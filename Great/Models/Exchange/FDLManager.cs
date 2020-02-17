using DocumentFormat.OpenXml.Packaging;
using GalaSoft.MvvmLight.Messaging;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Models.Interfaces;
using Great2.Utils;
using Great2.Utils.Extensions;
using Great2.Utils.Messages;
using Great2.ViewModels.Database;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms.Xfa;
using iText.Kernel.Pdf;
using Microsoft.Exchange.WebServices.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Great2.Models
{
    public class FDLManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private IProvider exchange;

        public FDLManager(IProvider provider)
        {
            exchange = provider;
            exchange.OnNewMessage += provider_OnNewMessage;
            exchange.OnMessageSent += Provider_OnMessageSent;
        }

        public bool IsExchangeAvailable()
        {
            return exchange.IsServiceAvailable();
        }

        private void provider_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            ProcessMessage(e.Message);
        }

        private void Provider_OnMessageSent(object sender, MessageEventArgs e)
        {
            switch (e.Message.Type)
            {
                case EEmailMessageType.SAP_Notification:
                    if (e.Message.DataInfo != null)
                    {
                        if (e.Message.DataInfo is FDLEVM)
                        {
                            FDLEVM fdl = e.Message.DataInfo as FDLEVM;
                            fdl.EStatus = EFDLStatus.Waiting;
                            fdl.NotifyAsNew = false;
                            fdl.LastSAPSendTimestamp = DateTime.Now.ToUnixTimestamp();
                            fdl.Save();
                        }
                        else if (e.Message.DataInfo is ExpenseAccountEVM)
                        {
                            ExpenseAccountEVM ea = e.Message.DataInfo as ExpenseAccountEVM;
                            ea.EStatus = ea.IsExcel ? EFDLStatus.Accepted : EFDLStatus.Waiting;
                            ea.NotifyAsNew = false;
                            ea.LastSAPSendTimestamp = DateTime.Now.ToUnixTimestamp();
                            ea.Save();
                        }
                    }
                    break;

                case EEmailMessageType.Cancellation_Request:
                    if (e.Message.DataInfo != null && e.Message.DataInfo is FDLEVM)
                    {
                        FDLEVM fdl = e.Message.DataInfo as FDLEVM;
                        fdl.EStatus = EFDLStatus.Cancelled;
                        fdl.NotifyAsNew = false;
                        fdl.Save();

                        using (DBArchive db = new DBArchive())
                        {
                            var relatedEAs = db.ExpenseAccounts.Where(ea => ea.FDL == fdl.Id);

                            foreach (var ea in relatedEAs)
                            {
                                ExpenseAccountEVM eavm = new ExpenseAccountEVM(ea);
                                eavm.EStatus = EFDLStatus.Cancelled;
                                eavm.NotifyAsNew = false;
                                eavm.Save(db);

                                // notify status change on gui
                                Messenger.Default.Send(new ItemChangedMessage<ExpenseAccountEVM>(this, eavm));
                            }
                        }
                    }
                    break;
                default:
                    break;


            }

            Messenger.Default.Send(new ProviderEmailSentMessage<EmailMessageDTO>(this, e.Message));
        }
        
        public bool CreateVirtualFdl(string Id, long StartDay, int WeekNr, int Order, bool IsExtra)
        {
            try
            {
                using (DBArchive db = new DBArchive())
                {
                    if (Id == null || Id.Length != 10 || Convert.ToInt32(Id.Substring(0, 4)) < ApplicationSettings.Timesheets.MinYear || WeekNr < 1 || WeekNr > 52 || db.FDLs.Any(fdl => fdl.Id == Id))
                        return false;

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        FDLEVM virtualFDL = new FDLEVM()
                        {
                            Id = Id,
                            StartDay = StartDay,
                            WeekNr = WeekNr,
                            Order = Order,
                            IsExtra = IsExtra,
                            IsVirtual = true,
                            EStatus = EFDLStatus.Accepted,
                            NotifyAsNew = true,
                            FileName = ""
                        };

                        if (virtualFDL.Save(db))
                        {
                            ExpenseAccountEVM virtualEA = new ExpenseAccountEVM()
                            {
                                FDL = virtualFDL.Id,
                                EStatus = EFDLStatus.Accepted,
                                IsVirtual = true,
                                NotifyAsNew = true,
                                FileName = ""
                            };

                            if (virtualEA.Save(db))
                            {
                                virtualFDL.Refresh(db);
                                virtualEA.Refresh(db);
                                transaction.Commit();

                                Messenger.Default.Send(new NewItemMessage<FDLEVM>(this, virtualFDL));
                                Messenger.Default.Send(new NewItemMessage<ExpenseAccountEVM>(this, virtualEA));
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: Log here
            }

            return false;
        }

        public ExpenseAccountEVM ImportEAFromFile(string filePath, bool NotifyAsNew = true, bool ExcludeExpense = false, bool OverrideIfExist = false)
        {
            ExpenseAccount ea = new ExpenseAccount();
            ExpenseAccountEVM eaEVM = null;
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(filePath));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();

                // only for testing purpose
                //using (StreamWriter writetext = new StreamWriter("c:\\test.txt"))
                //{
                //    foreach (PdfFormField f in fields.Values)
                //    {
                //        writetext.WriteLine($"Field: {f.GetFieldName()} Value: {f.GetValueAsString()}");
                //    }
                //}

                //General Info
                ea.FDL = fields[ApplicationSettings.ExpenseAccount.FieldNames.FDLNumber].GetValueAsString();
                ea.FileName = Path.GetFileName(filePath);
                ea.NotifyAsNew = NotifyAsNew;

                if (int.TryParse(fields[ApplicationSettings.ExpenseAccount.FieldNames.CdC].GetValueAsString(), out int cdc))
                    ea.CdC = cdc;

                string currency = fields[ApplicationSettings.ExpenseAccount.FieldNames.Currency].GetValueAsString();
                if (currency.Length > 4)
                    ea.Currency = currency.Substring(0, 4).Trim();

                string value = string.Empty;

                if (fields.ContainsKey(ApplicationSettings.ExpenseAccount.FieldNames.Notes))
                    value = fields[ApplicationSettings.ExpenseAccount.FieldNames.Notes].GetValueAsString().Trim();
                else if(fields.ContainsKey(ApplicationSettings.ExpenseAccount.FieldNames.Notes2))
                    value = fields[ApplicationSettings.ExpenseAccount.FieldNames.Notes2].GetValueAsString().Trim();

                ea.Notes = value != string.Empty ? value : null;

                using (DBArchive db = new DBArchive())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            ExpenseAccount tmpEA = db.ExpenseAccounts.SingleOrDefault(e => e.FileName == ea.FileName);

                            if (tmpEA != null && (tmpEA.IsVirtual || OverrideIfExist))
                            {
                                db.ExpenseAccounts.Remove(tmpEA);
                                db.SaveChanges();
                                tmpEA = null;
                            }

                            if (tmpEA == null)
                            {
                                if (NotifyAsNew && (ea.Currency == null || ea.Currency == string.Empty))
                                {
                                    ea.Currency = UserSettings.Localization.DefaultCurrency;
                                    ea.Currency1 = db.Currencies.SingleOrDefault(c => c.Id == ea.Currency);
                                }

                                db.ExpenseAccounts.Add(ea);
                                db.SaveChanges();

                                #region Expenses
                                if (!ExcludeExpense)
                                {
                                    foreach (var entry in ApplicationSettings.ExpenseAccount.FieldNames.ExpenseMatrix)
                                    {
                                        string type = fields[entry["Type"]].GetValueAsString().ToLower();

                                        if (string.IsNullOrEmpty(type))
                                            continue;

                                        var typeId = db.ExpenseTypes.Where(t => t.Description.ToLower() == type).Select(t => t.Id).FirstOrDefault();

                                        if (typeId == 0)
                                        {
                                            // EA from GREAT have wrong expense type description for hotels and fuels, so we need to find the correct match
                                            db.Entry(ea).Reference(p => p.FDL1).Load(); // explicit loading
                                            bool IsItaly = ea.FDL1?.Factory1?.TransferType == 0 || ea.FDL1?.Factory1?.TransferType == 1;
                                            typeId = db.ExpenseTypes.Where(t => t.Description.ToLower().Contains(type) && t.Description.ToLower().Contains(IsItaly ? "italia" : "estero")).Select(t => t.Id).FirstOrDefault();

                                            if (typeId == 0) // try last time with less filters as possible
                                                typeId = db.ExpenseTypes.Where(t => t.Description.ToLower().Contains(type)).Select(t => t.Id).FirstOrDefault();

                                            if (typeId == 0) // unknown expense type
                                                continue;
                                        }

                                        Expense expense = new Expense()
                                        {
                                            ExpenseAccount = ea.Id,
                                            Type = typeId
                                        };

                                        // convert string double value to invariant string double culture indipendent
                                        Func<string, string> ConvertToInvariantDoubleString = (str) => { return str.IndexOf('.') < str.IndexOf(',') ? str.Replace(".", "").Replace(",", ".") : str.Replace(",", ""); };

                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Mon_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out double amount))
                                            expense.MondayAmount = amount;
                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Tue_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                                            expense.TuesdayAmount = amount;
                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Wed_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                                            expense.WednesdayAmount = amount;
                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Thu_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                                            expense.ThursdayAmount = amount;
                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Fri_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                                            expense.FridayAmount = amount;
                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Sat_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                                            expense.SaturdayAmount = amount;
                                        if (double.TryParse(ConvertToInvariantDoubleString(fields[entry["Sun_Amount"]].GetValueAsString()), NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                                            expense.SundayAmount = amount;

                                        db.Expenses.Add(expense);
                                    }

                                    db.SaveChanges();
                                }
                                #endregion

                                transaction.Commit();

                                // Update all navigation properties
                                db.Entry(ea).Reference(p => p.FDL1).Load();
                                db.Entry(ea).Reference(p => p.FDLStatus).Load();
                                db.Entry(ea).Reference(p => p.Currency1).Load();
                                db.Entry(ea).Collection(p => p.Expenses).Load();

                                eaEVM = new ExpenseAccountEVM(ea);

                                Messenger.Default.Send(new NewItemMessage<ExpenseAccountEVM>(this, eaEVM));
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            eaEVM = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                eaEVM = null;
            }
            finally
            {
                pdfDoc?.Close();
            }

            return eaEVM;
        }

        public FDLEVM ImportFDLFromFile(string filePath, bool IsXfaPdf, bool NotifyAsNew = true, bool ExcludeTimesheets = false, bool ExcludeFactories = false, bool OverrideIfExist = false)
        {
            if (IsXfaPdf)
                return ImportFDL_XFAForm(filePath, NotifyAsNew, ExcludeTimesheets, ExcludeFactories, OverrideIfExist);
            else
                return ImportFDL_AcroForm(filePath, NotifyAsNew, ExcludeTimesheets, ExcludeFactories, OverrideIfExist);
        }

        public FDLEVM ImportFDL_XFAForm(string filePath, bool NotifyAsNew = true, bool ExcludeTimesheets = false, bool ExcludeFactories = false, bool OverrideIfExist = false)
        {
            FDL fdl = new FDL();
            FDLEVM fdlEVM = null;
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(filePath));
                PdfAcroForm Acroform = PdfAcroForm.GetAcroForm(pdfDoc, true);
                XfaForm XfaForm = Acroform.GetXfaForm();
                Func<string, string> GetFieldValue = (fieldName) => { return (XfaForm.FindDatasetsNode(fieldName) as XElement).Value; };

                // General info 
                fdl.Id = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.FDLNumber);
                fdl.FileName = Path.GetFileName(filePath);
                fdl.IsExtra = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.OrderType).Contains(ApplicationSettings.FDL.FDL_Extra);

                // Not yet implemented fields
                //fdl.EResult = GetFDLResultFromString(GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Result));
                //fdl.OutwardCar = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.OutwardCar) != null;
                //fdl.OutwardTaxi = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.OutwardTaxi) != null;
                //fdl.OutwardAircraft = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.OutwardAircraft) != null;
                //fdl.ReturnCar = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.ReturnCar) != null;
                //fdl.ReturnTaxi = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.ReturnTaxi) != null;
                //fdl.ReturnAircraft = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.ReturnAircraft) != null;

                long longResult;

                if (long.TryParse(GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Order), out longResult))
                    fdl.Order = longResult;

                fdl.NotifyAsNew = NotifyAsNew;

                //GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Cars1)
                //GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Cars2)

                // Not yet implemented fields
                //string value = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.PerformanceDescription).Trim();
                //fdl.PerformanceDescription = value != string.Empty ? value : null;

                //value = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.AssistantFinalTestResult).Trim();
                //fdl.ResultNotes = value != string.Empty ? value : null;

                //value = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.SoftwareVersionsOtherNotes).Trim();
                //fdl.Notes = value != string.Empty ? value : null;

                //value = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.PerformanceDescriptionDetails).Trim();
                //fdl.PerformanceDescriptionDetails = value != string.Empty ? value : null;

                // Extract week number
                foreach (var entry in ApplicationSettings.FDL.XFAFieldNames.Days)
                {
                    string day = GetFieldValue(entry.Value);

                    if (day != string.Empty)
                    {
                        DateTime date = DateTime.Now.ParseMultiCulture(day);
                        fdl.WeekNr = date.WeekNr();
                        fdl.StartDay = date.ToUnixTimestamp();
                        break;
                    }
                }

                if (fdl.WeekNr == 0)
                    throw new InvalidOperationException("Impossible to retrieve the week number.");

                // Save
                using (DBArchive db = new DBArchive())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            FDL tmpFdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);
                            Factory factory = null;
                            bool IsNewFactory = false;

                            if (tmpFdl != null && (tmpFdl.IsVirtual || OverrideIfExist))
                            {
                                db.FDLs.Remove(tmpFdl);
                                db.SaveChanges();
                                tmpFdl = null;
                            }

                            if (tmpFdl == null)
                            {
                                #region Automatic factories creation/assignment
                                if (!ExcludeFactories)
                                {
                                    string customer = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Customer);
                                    string address = GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Address);

                                    if (address != string.Empty && customer != string.Empty)
                                    {
                                        // try to associate the correct factory
                                        factory = GetFactory(db, fdl, address, customer);

                                        if (factory == null && UserSettings.Advanced.AutoAddFactories)
                                        {
                                            factory = new Factory()
                                            {
                                                Name = customer.Left(ApplicationSettings.Factories.FactoryNameMaxLength),
                                                CompanyName = customer.Left(ApplicationSettings.Factories.CompanyNameMaxLength),
                                                Address = address.Left(ApplicationSettings.Factories.AddressMaxLength),
                                                NotifyAsNew = true
                                            };

                                            db.Factories.Add(factory);
                                            db.SaveChanges();
                                            IsNewFactory = true;
                                        }

                                        if (factory != null && UserSettings.Advanced.AutoAssignFactories)
                                            fdl.Factory = factory.Id;
                                    }
                                }
                                #endregion

                                db.FDLs.Add(fdl);
                                db.SaveChanges();

                                #region Timesheets
                                if (!ExcludeTimesheets)
                                {
                                    foreach (var entry in ApplicationSettings.FDL.XFAFieldNames.TimesMatrix)
                                    {
                                        string strDate = GetFieldValue(entry.Value["Date"]);

                                        if (strDate != string.Empty)
                                        {
                                            DayEVM day = new DayEVM();
                                            day.Date = DateTime.Now.ParseMultiCulture(strDate);
                                            day.EType = EDayType.WorkDay;

                                            TimesheetEVM timesheet = new TimesheetEVM();
                                            timesheet.FDL = fdl.Id;
                                            timesheet.Date = DateTime.Now.ParseMultiCulture(strDate);

                                            TimeSpan time;

                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["TravelStartTimeAM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelStartTimeAM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["WorkStartTimeAM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkStartTimeAM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["WorkEndTimeAM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkEndTimeAM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["TravelEndTimeAM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelEndTimeAM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["TravelStartTimePM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelStartTimePM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["WorkStartTimePM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkStartTimePM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["WorkEndTimePM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkEndTimePM_t = time;
                                            if (TimeSpan.TryParse(GetFieldValue(entry.Value["TravelEndTimePM"]).Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelEndTimePM_t = time;

                                            // hack for supporting old travel periods
                                            if(timesheet.TravelStartTimeAM_t.HasValue && !timesheet.TravelEndTimeAM_t.HasValue &&
                                               !timesheet.WorkStartTimeAM_t.HasValue && !timesheet.WorkEndTimeAM_t.HasValue &&                                                 
                                               !timesheet.WorkStartTimePM_t.HasValue && !timesheet.WorkEndTimePM_t.HasValue &&
                                               !timesheet.TravelStartTimePM_t.HasValue && timesheet.TravelEndTimePM_t.HasValue)
                                            {
                                                if (timesheet.TravelEndTimePM_t < timesheet.TravelStartTimeAM_t)
                                                    timesheet.TravelEndTimeAM_t = new TimeSpan(0, 0, 0);
                                                else if (timesheet.TravelEndTimePM_t.Value.Ticks - timesheet.TravelStartTimeAM_t.Value.Ticks == 0)
                                                    timesheet.TravelEndTimeAM_t = new TimeSpan(12, 0, 0);
                                                else
                                                    timesheet.TravelEndTimeAM_t = timesheet.TravelStartTimeAM_t.Value + TimeSpan.FromTicks((timesheet.TravelEndTimePM_t.Value.Ticks - timesheet.TravelStartTimeAM_t.Value.Ticks) / 2);

                                                timesheet.TravelStartTimePM_t = timesheet.TravelEndTimeAM_t;
                                            }

                                            // hack for missing midnight value on fdl
                                            if (!timesheet.TravelStartTimeAM_t.HasValue && !timesheet.WorkStartTimeAM_t.HasValue && !timesheet.WorkEndTimeAM_t.HasValue && timesheet.TravelEndTimeAM_t.HasValue)
                                                timesheet.TravelStartTimeAM_t = new TimeSpan(0, 0, 0);
                                            if (!timesheet.WorkStartTimeAM_t.HasValue && timesheet.WorkEndTimeAM_t.HasValue)
                                                timesheet.WorkStartTimeAM_t = new TimeSpan(0, 0, 0);

                                            if (timesheet.TimePeriods != null)
                                            {
                                                day.Save(db);
                                                timesheet.Save(db);
                                            }
                                        }
                                    }
                                }
                                #endregion

                                // sUpdate all navigation properties
                                db.Entry(fdl).Reference(p => p.FDLResult).Load();
                                db.Entry(fdl).Reference(p => p.FDLStatus).Load();
                                db.Entry(fdl).Collection(p => p.Timesheets).Load();

                                // TODO: gestione segnalazione in caso di errori
                                fdlEVM = new FDLEVM(fdl);
                                if (fdlEVM.IsValid)
                                {
                                    transaction.Commit();

                                    if (IsNewFactory) Messenger.Default.Send(new NewItemMessage<FactoryEVM>(this, new FactoryEVM(factory)));
                                    Messenger.Default.Send(new NewItemMessage<FDLEVM>(this, fdlEVM));
                                }
                                else
                                {
                                    transaction.Rollback();
                                    fdlEVM = null;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            fdlEVM = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                fdlEVM = null;
            }
            finally
            {
                pdfDoc?.Close();
            }

            return fdlEVM;
        }

        public FDLEVM ImportFDL_AcroForm(string filePath, bool NotifyAsNew = true, bool ExcludeTimesheets = false, bool ExcludeFactories = false, bool OverrideIfExist = false)
        {
            FDL fdl = new FDL();
            FDLEVM fdlEVM = null;
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(filePath));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();

                // only for testing purpose
                //using (StreamWriter writetext = new StreamWriter("c:\\test.txt"))
                //{
                //    foreach (PdfFormField f in fields.Values)
                //    {
                //        writetext.WriteLine($"Field: {f.GetFieldName()} Value: {f.GetValueAsString()}");
                //    }
                //}

                // General info 
                fdl.Id = fields[ApplicationSettings.FDL.FieldNames.FDLNumber].GetValueAsString();
                fdl.FileName = Path.GetFileName(filePath);
                fdl.IsExtra = fields[ApplicationSettings.FDL.FieldNames.OrderType].GetValueAsString().Contains(ApplicationSettings.FDL.FDL_Extra);
                fdl.Result = (long)GetFDLResultFromString(fields[ApplicationSettings.FDL.FieldNames.Result].GetValueAsString());
                fdl.OutwardCar = fields[ApplicationSettings.FDL.FieldNames.OutwardCar].GetValue() != null;
                fdl.OutwardTaxi = fields[ApplicationSettings.FDL.FieldNames.OutwardTaxi].GetValue() != null;
                fdl.OutwardAircraft = fields[ApplicationSettings.FDL.FieldNames.OutwardAircraft].GetValue() != null;
                fdl.ReturnCar = fields[ApplicationSettings.FDL.FieldNames.ReturnCar].GetValue() != null;
                fdl.ReturnTaxi = fields[ApplicationSettings.FDL.FieldNames.ReturnTaxi].GetValue() != null;
                fdl.ReturnAircraft = fields[ApplicationSettings.FDL.FieldNames.ReturnAircraft].GetValue() != null;
                fdl.NotifyAsNew = NotifyAsNew;

                if (long.TryParse(fields[ApplicationSettings.FDL.FieldNames.Order].GetValueAsString(), out long longResult))
                    fdl.Order = longResult;

                //fields[ApplicationSettings.FDL.FieldNames.Cars1]
                //fields[ApplicationSettings.FDL.FieldNames.Cars2]

                string value = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescription].GetValueAsString().Trim();
                fdl.PerformanceDescription = value != string.Empty ? value : null;

                value = fields[ApplicationSettings.FDL.FieldNames.AssistantFinalTestResult].GetValueAsString().Trim();
                fdl.ResultNotes = value != string.Empty ? value : null;

                value = fields[ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes].GetValueAsString().Trim();
                fdl.Notes = value != string.Empty ? value : null;

                if (fields.ContainsKey(ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails))
                    value = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails].GetValueAsString().Trim();
                else if (fields.ContainsKey(ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails_old)) // Some very old FDL have a different field name for performance description details
                    value = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails_old].GetValueAsString().Trim();

                fdl.PerformanceDescriptionDetails = value != string.Empty ? value : null;

                // Extract week number
                foreach (var entry in ApplicationSettings.FDL.FieldNames.Days)
                {
                    string day = fields[entry.Value].GetValueAsString();

                    if (day != string.Empty)
                    {
                        DateTime date = DateTime.Now.ParseMultiCulture(day);
                        fdl.WeekNr = date.WeekNr();
                        fdl.StartDay = date.ToUnixTimestamp();
                        break;
                    }
                }

                if (fdl.WeekNr == 0)
                    throw new InvalidOperationException("Impossible to retrieve the week number.");

                // Save
                using (DBArchive db = new DBArchive())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            FDL tmpFdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);
                            Factory factory = null;
                            bool IsNewFactory = false;

                            if (tmpFdl != null && (tmpFdl.IsVirtual || OverrideIfExist))
                            {
                                db.FDLs.Remove(tmpFdl);
                                db.SaveChanges();
                                tmpFdl = null;
                            }

                            if (tmpFdl == null)
                            {
                                #region Automatic factories creation/assignment
                                if (!ExcludeFactories)
                                {
                                    string customer = fields[ApplicationSettings.FDL.FieldNames.Customer].GetValueAsString();
                                    string address = fields[ApplicationSettings.FDL.FieldNames.Address].GetValueAsString();

                                    if (address != string.Empty && customer != string.Empty)
                                    {
                                        // try to associate the correct factory
                                        factory = GetFactory(db, fdl, address, customer);

                                        if (factory == null && UserSettings.Advanced.AutoAddFactories)
                                        {
                                            factory = new Factory()
                                            {
                                                Name = customer.Left(ApplicationSettings.Factories.FactoryNameMaxLength),
                                                CompanyName = customer.Left(ApplicationSettings.Factories.CompanyNameMaxLength),
                                                Address = address.Left(ApplicationSettings.Factories.AddressMaxLength),
                                                NotifyAsNew = true
                                            };

                                            db.Factories.Add(factory);
                                            db.SaveChanges();
                                            IsNewFactory = true;
                                        }

                                        if (factory != null && UserSettings.Advanced.AutoAssignFactories)
                                            fdl.Factory = factory.Id;
                                    }
                                }
                                #endregion

                                db.FDLs.Add(fdl);
                                db.SaveChanges();

                                #region Timesheets
                                if (!ExcludeTimesheets)
                                {
                                    foreach (KeyValuePair<DayOfWeek, Dictionary<string, string>> entry in ApplicationSettings.FDL.FieldNames.TimesMatrix)
                                    {
                                        string strDate = fields[entry.Value["Date"]].GetValueAsString();

                                        if (strDate != string.Empty)
                                        {
                                            DayEVM day = new DayEVM();
                                            day.Date = DateTime.Now.ParseMultiCulture(strDate);
                                            day.EType = EDayType.WorkDay;

                                            TimesheetEVM timesheet = new TimesheetEVM();
                                            timesheet.FDL = fdl.Id;
                                            timesheet.Date = DateTime.Now.ParseMultiCulture(strDate);

                                            TimeSpan time;

                                            if (TimeSpan.TryParse(fields[entry.Value["TravelStartTimeAM"]].GetValueAsString().Replace("24", "00").Replace('.',':'), out time))
                                                timesheet.TravelStartTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkStartTimeAM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkStartTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkEndTimeAM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkEndTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["TravelEndTimeAM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelEndTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["TravelStartTimePM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelStartTimePM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkStartTimePM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkStartTimePM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkEndTimePM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.WorkEndTimePM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["TravelEndTimePM"]].GetValueAsString().Replace("24", "00").Replace('.', ':'), out time))
                                                timesheet.TravelEndTimePM_t = time;

                                            // hack for supporting old travel periods
                                            if (timesheet.TravelStartTimeAM_t.HasValue && !timesheet.TravelEndTimeAM_t.HasValue &&
                                               !timesheet.WorkStartTimeAM_t.HasValue && !timesheet.WorkEndTimeAM_t.HasValue &&
                                               !timesheet.WorkStartTimePM_t.HasValue && !timesheet.WorkEndTimePM_t.HasValue &&
                                               !timesheet.TravelStartTimePM_t.HasValue && timesheet.TravelEndTimePM_t.HasValue)
                                            {
                                                if (timesheet.TravelEndTimePM_t < timesheet.TravelStartTimeAM_t)
                                                    timesheet.TravelEndTimeAM_t = new TimeSpan(0, 0, 0);
                                                else if (timesheet.TravelEndTimePM_t.Value.Ticks - timesheet.TravelStartTimeAM_t.Value.Ticks == 0)
                                                    timesheet.TravelEndTimeAM_t = new TimeSpan(12, 0, 0);
                                                else
                                                    timesheet.TravelEndTimeAM_t = timesheet.TravelStartTimeAM_t.Value + TimeSpan.FromTicks((timesheet.TravelEndTimePM_t.Value.Ticks - timesheet.TravelStartTimeAM_t.Value.Ticks) / 2);

                                                timesheet.TravelStartTimePM_t = timesheet.TravelEndTimeAM_t;
                                            }

                                            // hack for missing midnight value on fdl
                                            if (!timesheet.TravelStartTimeAM_t.HasValue && !timesheet.WorkStartTimeAM_t.HasValue && !timesheet.WorkEndTimeAM_t.HasValue && timesheet.TravelEndTimeAM_t.HasValue)
                                                timesheet.TravelStartTimeAM_t = new TimeSpan(0, 0, 0);
                                            if (!timesheet.WorkStartTimeAM_t.HasValue && timesheet.WorkEndTimeAM_t.HasValue)
                                                timesheet.WorkStartTimeAM_t = new TimeSpan(0, 0, 0);

                                            if (timesheet.TimePeriods != null)
                                            {
                                                day.Save(db);
                                                timesheet.Save(db);
                                            }
                                        }
                                    }
                                }
                                #endregion

                                // sUpdate all navigation properties
                                db.Entry(fdl).Reference(p => p.FDLResult).Load();
                                db.Entry(fdl).Reference(p => p.FDLStatus).Load();
                                db.Entry(fdl).Collection(p => p.Timesheets).Load();

                                // TODO: gestione segnalazione in caso di errori
                                fdlEVM = new FDLEVM(fdl);
                                if (fdlEVM.IsValid)
                                {
                                    transaction.Commit();

                                    if (IsNewFactory)
                                        Messenger.Default.Send(new NewItemMessage<FactoryEVM>(this, new FactoryEVM(factory)));

                                    Messenger.Default.Send(new NewItemMessage<FDLEVM>(this, fdlEVM));
                                }
                                else
                                {
                                    transaction.Rollback();
                                    fdlEVM = null;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            fdlEVM = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                fdlEVM = null;
            }
            finally
            {
                pdfDoc?.Close();
            }

            return fdlEVM;
        }

        private Factory GetFactory(DBArchive db, FDL fdl, string address, string customer)
        {
            // 1) try with the exact address match
            Factory factory = db.Factories.SingleOrDefault(f => f.Address.ToLower() == address.ToLower());

            // 2) try if there are other FDL with the same order
            if (factory == null && fdl.Order != 0)
            {
                factory = (from f in db.Factories
                           from d in db.FDLs
                           where d.Order == fdl.Order && d.Factory == f.Id
                           select f).FirstOrDefault();
            }

            // 3) try if the address words are similar to the factory address
            if (factory == null)
            {
                try
                {
                    string[] newAddress = address.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Dictionary<long, int> factoryMatchRate = new Dictionary<long, int>();

                    foreach (Factory f in db.Factories)
                    {
                        int matchCount = 0;
                        string[] exsAddress = f.Address.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string word in newAddress)
                        {
                            if (word.Trim() != string.Empty && exsAddress.Any(x => x == word))
                                matchCount++;
                        }

                        if (matchCount > 0)
                            factoryMatchRate.Add(f.Id, (matchCount * 100) / newAddress.Count());
                    }

                    if (factoryMatchRate.Count > 0)
                    {
                        // get the factory with the highest match rate
                        var factoryRate = factoryMatchRate.Aggregate((l, r) => l.Value > r.Value ? l : r);

                        if (factoryRate.Value > 50)
                            factory = db.Factories.SingleOrDefault(f => f.Id == factoryRate.Key);
                    }
                }
                catch { }
            }

            return factory;
        }

        private Dictionary<string, string> GetXFAFormFields(XfaForm form)
        {
            // this is an hack for display fixed fields on saved read only FDL
            Dictionary<string, string> fields = new Dictionary<string, string>();
            Func<string, string> GetFieldValue = (fieldName) => { return (form.FindDatasetsNode(fieldName) as XElement).Value; };

            fields.Add(ApplicationSettings.FDL.FieldNames.FDLNumber, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.FDLNumber));
            fields.Add(ApplicationSettings.FDL.FieldNames.Order, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Order));
            fields.Add(ApplicationSettings.FDL.FieldNames.OrderType, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.OrderType));
            fields.Add(ApplicationSettings.FDL.FieldNames.Customer, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Customer));
            fields.Add(ApplicationSettings.FDL.FieldNames.Address, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Address));
            fields.Add(ApplicationSettings.FDL.FieldNames.CID, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.CID));
            fields.Add(ApplicationSettings.FDL.FieldNames.Technician, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Technician));
            fields.Add(ApplicationSettings.FDL.FieldNames.RequestedBy, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.RequestedBy));
            fields.Add(ApplicationSettings.FDL.FieldNames.AssistanceDescription, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.AssistanceDescription));

            fields.Add(ApplicationSettings.FDL.FieldNames.FDLNumber2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.FDLNumber));
            fields.Add(ApplicationSettings.FDL.FieldNames.Order2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Order));
            fields.Add(ApplicationSettings.FDL.FieldNames.OrderType2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.OrderType));
            fields.Add(ApplicationSettings.FDL.FieldNames.Customer2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Customer));
            fields.Add(ApplicationSettings.FDL.FieldNames.Address2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Address));
            fields.Add(ApplicationSettings.FDL.FieldNames.CID2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.CID));
            fields.Add(ApplicationSettings.FDL.FieldNames.Technician2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.Technician));
            fields.Add(ApplicationSettings.FDL.FieldNames.RequestedBy2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.RequestedBy));
            fields.Add(ApplicationSettings.FDL.FieldNames.AssistanceDescription2, GetFieldValue(ApplicationSettings.FDL.XFAFieldNames.AssistanceDescription));


            var AcroTimes = ApplicationSettings.FDL.FieldNames.TimesMatrix.Values.ToList();
            var XFATimes = ApplicationSettings.FDL.XFAFieldNames.TimesMatrix.Values.ToList();

            for (int i = 0; i < AcroTimes.Count && i < XFATimes.Count; i++)
            {
                string strDate = GetFieldValue(XFATimes[i]["Date"]);

                if (!string.IsNullOrEmpty(strDate))
                {
                    DateTime date = DateTime.ParseExact(strDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    fields.Add(AcroTimes[i]["Date"], date.ToString("dd/MMM/yy"));
                }
            }

            return fields;
        }

        private Dictionary<string, string> GetAcroFormFields(IFDLFile file, bool IsReadonly = false)
        {
            if (file is FDLEVM)
                return GetAcroFormFields(file as FDLEVM);
            else if (file is ExpenseAccountEVM)
                return GetAcroFormFields(file as ExpenseAccountEVM, IsReadonly);
            else
                return null;
        }

        private Dictionary<string, string> GetAcroFormFields(FDLEVM fdl)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            if (fdl.Factory1?.OverrideAddressOnFDL == true)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Address, fdl.Factory1.Address);
                fields.Add(ApplicationSettings.FDL.FieldNames.Address2, fdl.Factory1.Address);
            }

            foreach (var entry in ApplicationSettings.FDL.FieldNames.TimesMatrix)
            {
                TimesheetEVM timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == entry.Key);

                fields.Add(entry.Value["TravelStartTimeAM"], timesheet != null && timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["WorkStartTimeAM"], timesheet != null && timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["WorkEndTimeAM"], timesheet != null && timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["TravelEndTimeAM"], timesheet != null && timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["TravelStartTimePM"], timesheet != null && timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["WorkStartTimePM"], timesheet != null && timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["WorkEndTimePM"], timesheet != null && timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(entry.Value["TravelEndTimePM"], timesheet != null && timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }

            //TODO: pensare a come compilare i campi delle auto, se farlo in automatico oppure se farle selezionare dall'utente
            //fields.Add(ApplicationSettings.FDL.FieldNames.Cars1,
            //fields.Add(ApplicationSettings.FDL.FieldNames.Cars2,

            fields.Add(ApplicationSettings.FDL.FieldNames.OutwardCar, fdl.OutwardCar ? "1" : "0");
            fields.Add(ApplicationSettings.FDL.FieldNames.OutwardTaxi, fdl.OutwardTaxi ? "1" : "0");
            fields.Add(ApplicationSettings.FDL.FieldNames.OutwardAircraft, fdl.OutwardAircraft ? "1" : "0");

            fields.Add(ApplicationSettings.FDL.FieldNames.ReturnCar, fdl.ReturnCar ? "1" : "0");
            fields.Add(ApplicationSettings.FDL.FieldNames.ReturnTaxi, fdl.ReturnTaxi ? "1" : "0");
            fields.Add(ApplicationSettings.FDL.FieldNames.ReturnAircraft, fdl.ReturnAircraft ? "1" : "0");

            fields.Add(ApplicationSettings.FDL.FieldNames.PerformanceDescription, fdl.PerformanceDescription != null ? fdl.PerformanceDescription : string.Empty);
            fields.Add(ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails, fdl.PerformanceDescriptionDetails != null ? fdl.PerformanceDescriptionDetails : string.Empty);

            switch (fdl.Result)
            {
                case 1:
                    fields.Add(ApplicationSettings.FDL.FieldNames.Result, ApplicationSettings.FDL.Positive);
                    break;
                case 2:
                    fields.Add(ApplicationSettings.FDL.FieldNames.Result, ApplicationSettings.FDL.Negative);
                    break;
                case 3:
                    fields.Add(ApplicationSettings.FDL.FieldNames.Result, ApplicationSettings.FDL.WithReserve);
                    break;
                default:
                    break;
            }

            fields.Add(ApplicationSettings.FDL.FieldNames.AssistantFinalTestResult, fdl.ResultNotes ?? string.Empty);
            fields.Add(ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes, fdl.Notes ?? string.Empty);

            return fields;
        }

        private Dictionary<string, string> GetAcroFormFields(ExpenseAccountEVM ea, bool IsReadonly = false)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();

            var expenses = ea.Expenses.ToList();

            for (int i = 0; i < ApplicationSettings.ExpenseAccount.FieldNames.ExpenseMatrix.Count(); i++)
            {
                var entry = ApplicationSettings.ExpenseAccount.FieldNames.ExpenseMatrix[i];

                ExpenseEVM expense = null;

                if (i < expenses.Count())
                    expense = expenses[i];

                fields.Add(entry["Type"], expense != null ? expense.ExpenseType.Description : string.Empty);
                fields.Add(entry["Mon_Amount"], expense != null && expense.MondayAmount.HasValue ? expenses[i].MondayAmount.Value.ToString() : string.Empty);
                fields.Add(entry["Tue_Amount"], expense != null && expense.TuesdayAmount.HasValue ? expenses[i].TuesdayAmount.Value.ToString() : string.Empty);
                fields.Add(entry["Wed_Amount"], expense != null && expense.WednesdayAmount.HasValue ? expenses[i].WednesdayAmount.Value.ToString() : string.Empty);
                fields.Add(entry["Thu_Amount"], expense != null && expense.ThursdayAmount.HasValue ? expenses[i].ThursdayAmount.Value.ToString() : string.Empty);
                fields.Add(entry["Fri_Amount"], expense != null && expense.FridayAmount.HasValue ? expenses[i].FridayAmount.Value.ToString() : string.Empty);
                fields.Add(entry["Sat_Amount"], expense != null && expense.SaturdayAmount.HasValue ? expenses[i].SaturdayAmount.Value.ToString() : string.Empty);
                fields.Add(entry["Sun_Amount"], expense != null && expense.SundayAmount.HasValue ? expenses[i].SundayAmount.Value.ToString() : string.Empty);

                if (IsReadonly)
                    fields.Add(entry["Total"], expense.TotalAmount > 0 ? expenses[i].TotalAmount.ToString() : string.Empty);
            }

            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Currency, ea.Currency1 != null ? ea.Currency1.Description : string.Empty);

            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Notes, ea.Notes ?? string.Empty);

            if (ea.FDL1.Factory1?.OverrideAddressOnFDL == true)
            {
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Address, ea.FDL1.Factory1.Address);
            }

            if (IsReadonly)
            {
                //used to display totals on readonly file generation
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Mon, ea.MondayAmount > 0 ? ea.MondayAmount.ToString() : string.Empty);
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Tue, ea.TuesdayAmount > 0 ? ea.TuesdayAmount.ToString() : string.Empty);
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Wed, ea.WednesdayAmount > 0 ? ea.WednesdayAmount.ToString() : string.Empty);
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Thu, ea.ThursdayAmount > 0 ? ea.ThursdayAmount.ToString() : string.Empty);
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Fri, ea.FridayAmount > 0 ? ea.FridayAmount.ToString() : string.Empty);
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Sat, ea.SaturdayAmount > 0 ? ea.SaturdayAmount.ToString() : string.Empty);
                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total_Sun, ea.SundayAmount > 0 ? ea.SundayAmount.ToString() : string.Empty);

                fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.Total, ea.TotalAmount > 0 ? ea.TotalAmount.ToString() : string.Empty);
            }

            return fields;
        }

        private Dictionary<string, object> GetExcelFields(ExpenseAccountEVM ea)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();

            var expenses = ea.Expenses.ToList();

            for (int i = 0; i < ApplicationSettings.ExpenseAccount.FieldNames.xls_ExpenseMatrix.Count(); i++)
            {
                var entry = ApplicationSettings.ExpenseAccount.FieldNames.xls_ExpenseMatrix[i];

                ExpenseEVM expense = null;

                if (i < expenses.Count())
                    expense = expenses[i];

                if (expense != null)
                {
                    fields.Add(entry["Type"],  expense.ExpenseType.Description);

                    if (expense.MondayAmount.HasValue) fields.Add(entry["Mon_Amount"], expenses[i].MondayAmount.Value);
                    if (expense.TuesdayAmount.HasValue) fields.Add(entry["Tue_Amount"], expenses[i].TuesdayAmount.Value);
                    if (expense.WednesdayAmount.HasValue) fields.Add(entry["Wed_Amount"], expenses[i].WednesdayAmount.Value);
                    if (expense.ThursdayAmount.HasValue) fields.Add(entry["Thu_Amount"], expenses[i].ThursdayAmount.Value);
                    if (expense.FridayAmount.HasValue) fields.Add(entry["Fri_Amount"], expenses[i].FridayAmount.Value);
                    if (expense.SaturdayAmount.HasValue) fields.Add(entry["Sat_Amount"], expenses[i].SaturdayAmount.Value);
                    if (expense.SundayAmount.HasValue) fields.Add(entry["Sun_Amount"], expenses[i].SundayAmount.Value);
                }
            }

            if(ea.DaysOfWeek[0].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Mon_Date, ea.DaysOfWeek[0].Value);
            if(ea.DaysOfWeek[1].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Tue_Date, ea.DaysOfWeek[1].Value);
            if(ea.DaysOfWeek[2].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Wed_Date, ea.DaysOfWeek[2].Value);
            if(ea.DaysOfWeek[3].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Thu_Date, ea.DaysOfWeek[3].Value);
            if(ea.DaysOfWeek[4].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Fri_Date, ea.DaysOfWeek[4].Value);
            if(ea.DaysOfWeek[5].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Sat_Date, ea.DaysOfWeek[5].Value);
            if(ea.DaysOfWeek[6].HasValue) fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Sun_Date, ea.DaysOfWeek[6].Value);

            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Technician, UserSettings.Email.CachedDisplayName);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_CDC, ea.CdC);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_DateFrom, ea.DaysOfWeek.Where(d => d.HasValue).Select(d => d.Value).FirstOrDefault());
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_DateTo, ea.DaysOfWeek.Where(d => d.HasValue).Select(d => d.Value).LastOrDefault());
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Customer, ea.FDL1.Factory1.CompanyName);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Address, ea.FDL1.Factory1.Address);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_FDLNumber, ea.FDL);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Order, ea.FDL1.Order);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Currency, ea.Currency);
            fields.Add(ApplicationSettings.ExpenseAccount.FieldNames.xls_Notes, ea.Notes ?? string.Empty);

            return fields;
        }

        public void Compile(IFDLFile file, string destFileName)
        {
            try
            {
                switch(Path.GetExtension(file.FileName).ToLower())
                {
                    case ".pdf":
                        using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(file.FilePath), new PdfWriter(destFileName)))
                        {
                            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                            IDictionary<string, PdfFormField> fields = form.GetFormFields();

                            if (file is FDLEVM)
                            {
                                // this is an hack for display fixed fields on saved read only FDL
                                foreach (KeyValuePair<string, string> entry in GetXFAFormFields(form.GetXfaForm()))
                                {
                                    if (fields.ContainsKey(entry.Key))
                                        fields[entry.Key].SetValue(entry.Value);
                                }
                            }

                            foreach (KeyValuePair<string, string> entry in GetAcroFormFields(file, true))
                            {
                                if (fields.ContainsKey(entry.Key))
                                    fields[entry.Key].SetValue(entry.Value);
                            }
                        }
                        break;

                    case ".xlsx":
                        if(file is ExpenseAccountEVM)
                        {
                            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(destFileName, true))
                            {
                                foreach (KeyValuePair<string, object> entry in GetExcelFields(file as ExpenseAccountEVM))
                                {
                                    ExcelHelper.SetCellValue(doc, "Spese", entry.Key, entry.Value);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private void CompileXFDF(IFDLFile file, string FDLfileName, string XFDFFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;

            XmlNode docNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlNode xfdfNode = xmlDoc.CreateElement("xfdf", "http://ns.adobe.com/xfdf/");
            XmlNode fNode = xmlDoc.CreateElement("f", xfdfNode.NamespaceURI);
            XmlNode fieldsNode = xmlDoc.CreateElement("fields", xfdfNode.NamespaceURI);

            XmlAttribute space = xmlDoc.CreateAttribute("xml:space");
            XmlAttribute href = xmlDoc.CreateAttribute("href");

            space.Value = "preserve";
            href.Value = FDLfileName;

            xfdfNode.Attributes.Append(space);
            fNode.Attributes.Append(href);

            xmlDoc.AppendChild(docNode);
            xmlDoc.AppendChild(xfdfNode);
            xfdfNode.AppendChild(fNode);
            xfdfNode.AppendChild(fieldsNode);

            foreach (KeyValuePair<string, string> entry in GetAcroFormFields(file))
            {
                XmlNode fieldNode = xmlDoc.CreateElement("field", xfdfNode.NamespaceURI);
                XmlNode valueNode = xmlDoc.CreateElement("value", xfdfNode.NamespaceURI);

                XmlAttribute name = xmlDoc.CreateAttribute("name");
                name.Value = entry.Key;

                fieldNode.Attributes.Append(name);

                valueNode.InnerText = entry.Value;

                fieldNode.AppendChild(valueNode);
                fieldsNode.AppendChild(fieldNode);
            }

            xmlDoc.Save(XFDFFileName);
        }

        public bool SendToSAP(IFDLFile file, string[] attachments = null)
        {
            if (file == null)
                return false;

            EmailMessageDTO message = new EmailMessageDTO();
            message.Type = EEmailMessageType.SAP_Notification;
            message.DataInfo = file;
            message.Importance = Importance.High;
            message.ToRecipients.Add(ApplicationSettings.EmailRecipients.FDLSystem);

            if (file is FDLEVM)
            {
                FDLEVM fdl = file as FDLEVM;
                message.CcRecipients.Add(ApplicationSettings.EmailRecipients.HR);

                message.Subject = $"FDL {fdl.Id} - Factory {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")} - Order {fdl.Order}";

                using (DBArchive db = new DBArchive())
                {
                    var recipients = db.OrderEmailRecipients.Where(r => r.Order == fdl.Order).Select(r => r.Address);

                    foreach (var r in recipients)
                        message.CcRecipients.Add(r);
                }

            }
            else if (file is ExpenseAccountEVM)
            {
                ExpenseAccountEVM ea = file as ExpenseAccountEVM;
                message.Subject = $"Expense Account {ea.FDL} - Factory {(ea.FDL1.Factory1 != null ? ea.FDL1.Factory1.Name : "Unknown")} - Order {ea.FDL1.Order}";
            }

            message.Attachments.Add(file.FilePath);

            if(attachments != null)
            {
                foreach(string attachment in attachments)
                {
                    if(!message.Attachments.Contains(attachment))
                        message.Attachments.Add(attachment);
                }
            }

            return exchange.SendEmail(message);
        }

        public bool SendTo(string address, IFDLFile file)
        {
            if (file == null)
                return false;

            EmailMessageDTO message = new EmailMessageDTO();
            message.Type = EEmailMessageType.Message;

            if (file is FDLEVM)
            {
                FDLEVM fdl = file as FDLEVM;
                message.Subject = $"FDL {fdl.Id} - Factory {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")} - Order {fdl.Order}";
            }
            else if (file is ExpenseAccountEVM)
            {
                ExpenseAccountEVM ea = file as ExpenseAccountEVM;
                message.Subject = $"Expense Account {ea.FDL} - Factory {(ea.FDL1.Factory1 != null ? ea.FDL1.Factory1.Name : "Unknown")} - Order {ea.FDL1.Order}";
            }

            message.ToRecipients.Add(address);
            message.Attachments.Add(file.FilePath);

            return exchange.SendEmail(message);
        }

        public bool NewOutlookMessage(IFDLFile file)
        {
            if (file == null)
                return false;

            EmailMessageDTO message = new EmailMessageDTO();
            message.Type = EEmailMessageType.Message;

            if (file is FDLEVM)
            {
                FDLEVM fdl = file as FDLEVM;
                message.Subject = $"FDL {fdl.Id} - Factory {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")} - Order {fdl.Order}";
            }
            else if (file is ExpenseAccountEVM)
            {
                ExpenseAccountEVM ea = file as ExpenseAccountEVM;
                message.Subject = $"Expense Account {ea.FDL} - Factory {(ea.FDL1.Factory1 != null ? ea.FDL1.Factory1.Name : "Unknown")} - Order {ea.FDL1.Order}";

                foreach (string recipient in UserSettings.Email.Recipients.EANewMessageDefaultRecipients)
                {
                    if (!message.ToRecipients.Contains(recipient))
                        message.ToRecipients.Add(recipient);
                }
            }

            message.Attachments.Add(file.FilePath);

            return OutlookHelper.NewMessage(message);
        }

        public bool SendCancellationRequest(FDLEVM fdl)
        {
            if (fdl == null)
                return false;

            EmailMessageDTO message = new EmailMessageDTO();
            message.Type = EEmailMessageType.Cancellation_Request;
            message.DataInfo = fdl;
            message.Subject = $"Cancellation Request for FDL {fdl.Id}";
            message.Body = $@"Please, cancel the following FDL because it's not applicable.<br>
                                <br>
                                FDL: <b>{fdl.Id}</b><br>
                                Factory: {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")}<br>
                                Order: {fdl.Order}<br>
                                <br>
                                Thank you";
            message.Importance = Importance.High;

            foreach (string address in UserSettings.Email.Recipients.FDLCancelRequest)
                message.ToRecipients.Add(address);

            return exchange.SendEmail(message);
        }

        public bool SaveAs(IFDLFile file, string filePath)
        {
            if (file == null || filePath == string.Empty)
                return false;

            Compile(file, filePath);
            return true;
        }

        public bool CreateXFDF(IFDLFile file, out string FilePath)
        {
            FilePath = string.Empty;

            if (file == null)
                return false;

            FilePath = Path.GetDirectoryName(file.FilePath) + "\\" + Path.GetFileNameWithoutExtension(file.FilePath) + ".XFDF";
            CompileXFDF(file, file.FilePath, FilePath);
            return true;
        }

        private void ProcessMessage(EmailMessage message)
        {
            EMessageType type = GetMessageType(message);
            string fdlNumber = GetFDLNumber(message, type);

            switch (type)
            {
                case EMessageType.FDL_Accepted:
                    using (DBArchive db = new DBArchive())
                    {
                        FDL accepted = db.FDLs.SingleOrDefault(f => f.Id == fdlNumber);

                        if (accepted != null && accepted.Status != (long)EFDLStatus.Accepted)
                        {
                            if (message.DateTimeReceived < DateTime.Now.FromUnixTimestamp(accepted.LastSAPSendTimestamp ?? 0))
                                break;

                            accepted.Status = (long)EFDLStatus.Accepted;
                            accepted.LastError = null;
                            accepted.NotifyAsNew = false;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<FDLEVM>(this, new FDLEVM(accepted)));
                        }
                    }
                    break;
                case EMessageType.FDL_Rejected:
                    using (DBArchive db = new DBArchive())
                    {
                        FDL rejected = db.FDLs.SingleOrDefault(f => f.Id == fdlNumber);

                        if (rejected != null && rejected.Status != (long)EFDLStatus.Rejected && rejected.Status != (long)EFDLStatus.Accepted)
                        {
                            if (message.DateTimeReceived < DateTime.Now.FromUnixTimestamp(rejected.LastSAPSendTimestamp ?? 0))
                                break;

                            rejected.Status = (long)EFDLStatus.Rejected;
                            rejected.LastError = message.Body?.Text.Trim();
                            rejected.NotifyAsNew = false;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<FDLEVM>(this, new FDLEVM(rejected)));
                        }
                    }
                    break;
                case EMessageType.EA_Accepted:
                    using (DBArchive db = new DBArchive())
                    {
                        string filename = string.Empty;

                        // get the EA file name
                        foreach (Attachment attachment in message.Attachments)
                        {
                            if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                                continue;

                            filename = (attachment as FileAttachment).Name.ToLower();
                            break;
                        }

                        ExpenseAccount accepted = db.ExpenseAccounts.SingleOrDefault(ea => ea.FileName.ToLower() == filename);

                        if (accepted != null && accepted.Status != (long)EFDLStatus.Accepted)
                        {
                            if (message.DateTimeReceived < DateTime.Now.FromUnixTimestamp(accepted.LastSAPSendTimestamp ?? 0))
                                break;

                            accepted.Status = (long)EFDLStatus.Accepted;
                            accepted.LastError = null;
                            accepted.NotifyAsNew = false;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<ExpenseAccountEVM>(this, new ExpenseAccountEVM(accepted)));
                        }
                    }
                    break;
                case EMessageType.EA_Rejected:
                case EMessageType.EA_RejectedResubmission:
                    using (DBArchive db = new DBArchive())
                    {
                        string filename = string.Empty;

                        // get the EA file name
                        foreach (Attachment attachment in message.Attachments)
                        {
                            if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                                continue;

                            filename = (attachment as FileAttachment).Name.ToLower();
                            break;
                        }

                        ExpenseAccount expenseAccount = db.ExpenseAccounts.SingleOrDefault(ea => ea.FileName.ToLower() == filename);

                        if (expenseAccount != null && expenseAccount.Status != (long)EFDLStatus.Rejected && expenseAccount.Status != (long)EFDLStatus.Accepted)
                        {
                            if (message.DateTimeReceived < DateTime.Now.FromUnixTimestamp(expenseAccount.LastSAPSendTimestamp ?? 0))
                                break;

                            expenseAccount.Status = (long)EFDLStatus.Rejected;
                            expenseAccount.LastError = message.Body?.Text.Trim();
                            expenseAccount.NotifyAsNew = false;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<ExpenseAccountEVM>(this, new ExpenseAccountEVM(expenseAccount)));
                        }
                    }
                    break;
                case EMessageType.FDL_EA_New:
                    if (message.HasAttachments)
                    {
                        bool deleteMessage = false;

                        foreach (Attachment attachment in message.Attachments)
                        {
                            if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                                continue;

                            FileAttachment fileAttachment = attachment as FileAttachment;

                            switch (GetFileType(attachment.Name))
                            {
                                case EFileType.FDL:
                                    if (!File.Exists(ApplicationSettings.Directories.FDL + fileAttachment.Name))
                                    {
                                        bool exist = false;

                                        fileAttachment.Load(ApplicationSettings.Directories.FDL + fileAttachment.Name);

                                        using (DBArchive db = new DBArchive())
                                        {
                                            if (db.FDLs.SingleOrDefault(f => f.FileName.ToLower() == fileAttachment.Name.ToLower()) != null)
                                                exist = true;
                                        }

                                        if(!exist)
                                        {
                                            FDLEVM fdl = ImportFDLFromFile(ApplicationSettings.Directories.FDL + fileAttachment.Name, true, true, true);

                                            if (fdl == null)
                                            {
                                                File.Delete(ApplicationSettings.Directories.FDL + fileAttachment.Name);
                                                deleteMessage = true;
                                            }
                                            else if (UserSettings.Advanced.ExcelExpenseAccount)
                                            {
                                                ExpenseAccountEVM ea = new ExpenseAccountEVM();
                                                ea.FDL = fdl.Id;
                                                ea.CdC = UserSettings.Advanced.CDC;
                                                ea.NotifyAsNew = true;
                                                ea.Currency = UserSettings.Localization.DefaultCurrency;
                                                ea.FileName = Path.GetFileNameWithoutExtension(fdl.FileName) + " R.xlsx";

                                                File.WriteAllBytes(ea.FilePath, Properties.Resources.ExpenseAccount);

                                                using (DBArchive db = new DBArchive())
                                                {   
                                                    ea.Save(db);
                                                    ea.Refresh(db);
                                                    ea.InitDaysOfWeek();
                                                    ea.InitExpenses(db);
                                                    ea.InitExpensesByCountry(db);
                                                }

                                                Messenger.Default.Send(new NewItemMessage<ExpenseAccountEVM>(this, ea));
                                            }
                                        }
                                    }
                                    break;
                                case EFileType.ExpenseAccount:
                                    if (!UserSettings.Advanced.ExcelExpenseAccount && !File.Exists(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name))
                                    {
                                        bool exist = false;

                                        fileAttachment.Load(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name);

                                        using (DBArchive db = new DBArchive())
                                        {
                                            if (db.ExpenseAccounts.SingleOrDefault(e => e.FileName.ToLower() == fileAttachment.Name.ToLower()) != null)
                                                exist = true;
                                        }

                                        if (!exist)
                                        {
                                            ExpenseAccountEVM ea = ImportEAFromFile(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name, true, true);

                                            if (ea == null)
                                            {
                                                File.Delete(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name);
                                                deleteMessage = true;
                                            }
                                            else
                                            {
                                                using (DBArchive db = new DBArchive())
                                                {
                                                    ea.InitExpenses(db);
                                                    ea.InitExpensesByCountry(db);
                                                }
                                            }   
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (deleteMessage)
                                break;
                        }

                        if(deleteMessage)
                            message.Delete(DeleteMode.MoveToDeletedItems);
                    }
                    break;

                default:
                    break;
            }
        }

        private EMessageType GetMessageType(EmailMessage message)
        {
            if (message.Subject.Contains(ApplicationSettings.FDL.FDL_Accepted))
                return EMessageType.FDL_Accepted;
            else if (message.Subject.Contains(ApplicationSettings.FDL.FDL_Rejected))
                return EMessageType.FDL_Rejected;
            else if (message.Subject.Contains(ApplicationSettings.ExpenseAccount.EA_Accepted))
                return EMessageType.EA_Accepted;
            else if (message.Subject.Contains(ApplicationSettings.ExpenseAccount.EA_Rejected))
                return EMessageType.EA_Rejected;
            else if (message.Subject.Contains(ApplicationSettings.ExpenseAccount.EA_RejectedResubmission))
                return EMessageType.EA_RejectedResubmission;
            else if (message.Subject.Contains(ApplicationSettings.FDL.Reminder))
                return EMessageType.Reminder;
            else
            {
                if (!message.HasAttachments)
                    return EMessageType.Unknown;

                EFileType type = GetFileType(message.Attachments.FirstOrDefault().Name);

                if (type == EFileType.FDL || type == EFileType.ExpenseAccount)
                    return EMessageType.FDL_EA_New;
                else
                    return EMessageType.Unknown;
            }
        }

        public static EFileType GetFileType(string filename)
        {
            try
            {
                string[] words = Path.GetFileNameWithoutExtension(filename).Split(' ');

                if (words.Length > 5)
                {
                    string FDL = words[0];
                    string CID = words[words.Length - 4];
                    string WeekNr = words[words.Length - 3];
                    string Month = words[words.Length - 2];
                    string Year = words[words.Length - 1];

                    if (FDL.All(char.IsDigit))
                    {
                        if (words.LastOrDefault().Contains("R"))
                            return EFileType.ExpenseAccount;
                        else if (CID.All(char.IsDigit) &&
                                 WeekNr.All(char.IsDigit) && Enumerable.Range(1, 52).Contains(int.Parse(WeekNr)) &&
                                 Month.All(char.IsDigit) && Enumerable.Range(1, 12).Contains(int.Parse(Month)) &&
                                 Year.All(char.IsDigit) && Enumerable.Range(ApplicationSettings.Timesheets.MinYear, ApplicationSettings.Timesheets.MaxYear).Contains(int.Parse(Year)))
                            return EFileType.FDL;
                    }
                }
            }
            catch { }

            return EFileType.Unknown;
        }

        private string GetFDLNumber(EmailMessage message, EMessageType type)
        {
            string FDL = string.Empty;
            string[] words;

            try
            {
                if (message.HasAttachments)
                {
                    foreach (Attachment attachment in message.Attachments)
                    {
                        if (!(attachment is FileAttachment))
                            continue;

                        EFileType attType = GetFileType(attachment.Name);

                        if (attType == EFileType.Unknown)
                            continue;

                        words = Path.GetFileNameWithoutExtension(attachment.Name).Split(' ');

                        if (attType == EFileType.FDL)
                            FDL = $"{words[words.Length - 1]}/{words[0]}";
                        else if (attType == EFileType.ExpenseAccount)
                            FDL = $"{words[words.Length - 2]}/{words[0]}";
                        break;
                    }
                }

                if (FDL == string.Empty)
                {
                    // NB In case of missing attachments, there might be an incorrect result if you recive the message regarding an FDL of the previous year in the new year.
                    // there are no solutions for this kind of issue.                    

                    switch (type)
                    {
                        case EMessageType.FDL_Accepted:
                        case EMessageType.FDL_Rejected:
                            // INVALID FDL (XXXXX)
                            // FDL RECEIVED (XXXXX)
                            Match match = Regex.Match(message.Subject, @"\(([^)]*)\)");
                            if (match.Success || match.Groups.Count > 0)
                                FDL = $"{message.DateTimeSent.Year}/{match.Groups[1].Value}";
                            break;
                        case EMessageType.EA_Accepted:
                        case EMessageType.EA_Rejected:
                            // FDL XXXXX NOTA SPESE ACCETTATA
                            //  0    1    2     3       4
                            // FDL XXXXX NOTA SPESE RIFIUTATA
                            //  0    1    2     3       4
                            words = message.Subject.Split(' ');
                            if (words.Length > 1)
                                FDL = $"{message.DateTimeSent.Year}/{words[1]}";
                            break;
                        case EMessageType.EA_RejectedResubmission:
                            // Reinvio nota spese YYYY/XXXXX respinto
                            //    0      1    2       3         4
                            words = message.Subject.Split(' ');
                            if (words.Length > 3)
                                FDL = words[3];
                            break;
                        default:
                            break;
                    }
                }
            }
            catch { }

            return FDL;
        }

        private EFDLResult GetFDLResultFromString(string result)
        {
            switch (result)
            {
                case "1":
                    return EFDLResult.Positive;
                case "2":
                    return EFDLResult.Negative;
                case "3":
                    return EFDLResult.WithReserve;
                default:
                    return EFDLResult.None;
            }
        }
    }

    public enum EFileType
    {
        Unknown,
        FDL,
        ExpenseAccount
    }

    public enum EMessageType
    {
        Unknown,
        FDL_Accepted,
        FDL_Rejected,
        EA_Accepted,
        EA_Rejected,
        EA_RejectedResubmission,
        FDL_EA_New,
        Reminder
    }

    public enum EFDLStatus
    {
        New = 0,
        Waiting = 1,
        Accepted = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum EFDLResult
    {
        None = 0,
        Positive = 1,
        Negative = 2,
        WithReserve = 3
    }
}
