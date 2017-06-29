using GalaSoft.MvvmLight.Messaging;
using Great.Utils.Extensions;
using Great.Utils.Messages;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using Microsoft.Exchange.WebServices.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Great.Models
{
    public class FDLManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        MSExchangeProvider exchangeProvider;

        public FDLManager(MSExchangeProvider exProvider)
        {
            exchangeProvider = exProvider;
            exchangeProvider.OnNewMessage += ExchangeProvider_OnNewMessage;
        }

        private void ExchangeProvider_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            ProcessMessage(e.Message);

            //TEST
            //ImportEAFromFile("c:\\notaspese.pdf", false, false);
        }

        public ExpenseAccount ImportEAFromFile(string filePath, bool NotifyAsNew = true, bool ExcludeExpense = false, bool OverrideIfExist = false)
        {
            ExpenseAccount ea = new ExpenseAccount();
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(filePath));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();

                //General Info
                ea.FDL = fields[ApplicationSettings.ExpenseAccount.FieldNames.FDLNumber].GetValueAsString();
                ea.FileName = Path.GetFileName(filePath);
                ea.NotifyAsNew = NotifyAsNew;

                int cdc;
                if (int.TryParse(fields[ApplicationSettings.ExpenseAccount.FieldNames.CdC].GetValueAsString(), out cdc))
                    ea.CdC = cdc;

                string currency = fields[ApplicationSettings.ExpenseAccount.FieldNames.Currency].GetValueAsString();
                if (currency.Length > 4)
                    ea.Currency = currency.Substring(0, 4).Trim();

                //TODO: importazione spese

                using (DBEntities db = new DBEntities())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            ExpenseAccount tmpEA = db.ExpenseAccounts.SingleOrDefault(e => e.FDL == ea.FDL);
                            
                            if (tmpEA != null && OverrideIfExist)
                            {
                                db.ExpenseAccounts.Remove(tmpEA);
                                db.SaveChanges();
                                tmpEA = null;
                            }

                            if (tmpEA == null)
                            {
                                db.ExpenseAccounts.Add(ea);
                                db.SaveChanges();
                                transaction.Commit();
                                Messenger.Default.Send(new NewItemMessage<ExpenseAccount>(this, ea));
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ea = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ea = null;
            }
            finally
            {
                pdfDoc?.Close();
            }

            return ea;
        }

        public FDL ImportFDLFromFile(string filePath, bool NotifyAsNew = true, bool ExcludeTimesheets = false, bool ExcludeFactories = false, bool OverrideIfExist = false)
        {   
            FDL fdl = new FDL();
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(filePath));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();
                
                // General info 
                fdl.Id = fields[ApplicationSettings.FDL.FieldNames.FDLNumber].GetValueAsString();                
                fdl.Order = fields[ApplicationSettings.FDL.FieldNames.Order].GetValueAsString();
                fdl.FileName = Path.GetFileName(filePath);
                fdl.IsExtra = fields[ApplicationSettings.FDL.FieldNames.OrderType].GetValueAsString().Contains(ApplicationSettings.FDL.FDL_Extra);
                fdl.EResult = GetFDLResultFromString(fields[ApplicationSettings.FDL.FieldNames.Result].GetValueAsString());
                fdl.OutwardCar = fields[ApplicationSettings.FDL.FieldNames.OutwardCar].GetValue() != null;
                fdl.OutwardTaxi = fields[ApplicationSettings.FDL.FieldNames.OutwardTaxi].GetValue() != null;
                fdl.OutwardAircraft = fields[ApplicationSettings.FDL.FieldNames.OutwardAircraft].GetValue() != null;
                fdl.ReturnCar = fields[ApplicationSettings.FDL.FieldNames.ReturnCar].GetValue() != null;
                fdl.ReturnTaxi = fields[ApplicationSettings.FDL.FieldNames.ReturnTaxi].GetValue() != null;
                fdl.ReturnAircraft = fields[ApplicationSettings.FDL.FieldNames.ReturnAircraft].GetValue() != null;
                fdl.NotifyAsNew = NotifyAsNew;

                // TODO: gestire automobili
                //fields[ApplicationSettings.FDL.FieldNames.Cars1]
                //fields[ApplicationSettings.FDL.FieldNames.Cars2]

                string value = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescription].GetValueAsString().Trim();
                fdl.PerformanceDescription = value != string.Empty ? value : null;

                value = fields[ApplicationSettings.FDL.FieldNames.AssistantFinalTestResult].GetValueAsString().Trim();
                fdl.ResultNotes = value != string.Empty ? value : null;

                value = fields[ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes].GetValueAsString().Trim();
                fdl.Notes = value != string.Empty ? value : null;

                value = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails].GetValueAsString().Trim();
                fdl.PerformanceDescriptionDetails = value != string.Empty ? value : null;

                // Extract week number
                string[] week = new string[]
                {
                    fields[ApplicationSettings.FDL.FieldNames.Mon_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Tue_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Wed_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Thu_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Fri_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Sat_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Sun_Date].GetValueAsString()
                };

                foreach(string day in week)
                {
                    if (day != string.Empty)
                    {
                        fdl.WeekNr = DateTime.Parse(day).WeekNr();
                        break;
                    }
                }

                if (fdl.WeekNr == 0)
                    throw new InvalidOperationException("Impossible to retrieve the week number.");
                
                // Save
                using (DBEntities db = new DBEntities())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            FDL tmpFdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);
                            Factory factory = null;
                            bool IsNewFactory = false;

                            if (tmpFdl != null && OverrideIfExist)
                            {
                                db.FDLs.Remove(tmpFdl);
                                db.SaveChanges();
                                tmpFdl = null;
                            }

                            if (tmpFdl == null)
                            {
                                #region Automatic factories creation/assignment
                                if(!ExcludeFactories)
                                {
                                    string customer = fields[ApplicationSettings.FDL.FieldNames.Customer].GetValueAsString();
                                    string address = fields[ApplicationSettings.FDL.FieldNames.Address].GetValueAsString();

                                    if (address != string.Empty && customer != string.Empty)
                                    {
                                        //TODO: migliorare riconoscimento stabilimenti 
                                        factory = db.Factories.SingleOrDefault(f => f.Address.ToLower() == address.ToLower());

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

                                        if (UserSettings.Advanced.AutoAssignFactories)
                                            fdl.Factory = factory.Id;
                                    }
                                }                                
                                #endregion
                                
                                #region Timesheets
                                if(!ExcludeTimesheets)
                                {
                                    foreach (KeyValuePair<DayOfWeek, Dictionary<string, string>> entry in ApplicationSettings.FDL.FieldNames.TimesMatrix)
                                    {
                                        string strDate = fields[entry.Value["Date"]].GetValueAsString();

                                        if (strDate != string.Empty)
                                        {
                                            Day day = new Day();
                                            day.Date = DateTime.Parse(strDate);
                                            day.EType = EDayType.WorkDay;

                                            Timesheet timesheet = new Timesheet();
                                            timesheet.FDL = fdl.Id;
                                            timesheet.Date = DateTime.Parse(strDate);

                                            TimeSpan time;

                                            if (TimeSpan.TryParse(fields[entry.Value["TravelStartTimeAM"]].GetValueAsString(), out time))
                                                timesheet.TravelStartTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkStartTimeAM"]].GetValueAsString(), out time))
                                                timesheet.WorkStartTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkEndTimeAM"]].GetValueAsString(), out time))
                                                timesheet.WorkEndTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["TravelEndTimeAM"]].GetValueAsString(), out time))
                                                timesheet.TravelEndTimeAM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["TravelStartTimePM"]].GetValueAsString(), out time))
                                                timesheet.TravelStartTimePM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkStartTimePM"]].GetValueAsString(), out time))
                                                timesheet.WorkStartTimePM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["WorkEndTimePM"]].GetValueAsString(), out time))
                                                timesheet.WorkEndTimePM_t = time;
                                            if (TimeSpan.TryParse(fields[entry.Value["TravelEndTimePM"]].GetValueAsString(), out time))
                                                timesheet.TravelEndTimePM_t = time;

                                            if (timesheet.TimePeriods != null)
                                            {
                                                db.Days.AddOrUpdate(day);
                                                db.Timesheets.Add(timesheet);
                                            }
                                        }
                                    }
                                }
                                #endregion
                                
                                db.FDLs.Add(fdl);
                                db.SaveChanges();

                                // TODO: gestione segnalazione in caso di errori
                                if (fdl.IsValid)
                                {
                                    transaction.Commit();

                                    if (IsNewFactory) Messenger.Default.Send(new NewItemMessage<Factory>(this, factory));
                                    Messenger.Default.Send(new NewItemMessage<FDL>(this, fdl));
                                }
                                else
                                    transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            fdl = null;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                fdl = null;
            }
            finally
            {
                pdfDoc?.Close();
            }

            return fdl;
        }
        
        private Dictionary<string, string> GetAcroFormFields(FDL fdl)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            Timesheet timesheet = null;

            foreach (KeyValuePair<DayOfWeek, Dictionary<string, string>> entry in ApplicationSettings.FDL.FieldNames.TimesMatrix)
            {
                timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == entry.Key);

                if (timesheet != null)
                {
                    fields.Add(entry.Value["TravelStartTimeAM"], timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["WorkStartTimeAM"], timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["WorkEndTimeAM"], timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["TravelEndTimeAM"], timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["TravelStartTimePM"], timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["WorkStartTimePM"], timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["WorkEndTimePM"], timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    fields.Add(entry.Value["TravelEndTimePM"], timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                }
            }
           
            //TODO: pensare a come compilare i campi delle auto, se farlo in automatico oppure se farle selezionare dall'utente
            //fields.Add(ApplicationSettings.FDL.FieldNames.Cars1,
            //fields.Add(ApplicationSettings.FDL.FieldNames.Cars2,

            if (fdl.OutwardCar)
                fields.Add(ApplicationSettings.FDL.FieldNames.OutwardCar, "1");
            if (fdl.OutwardTaxi)
                fields.Add(ApplicationSettings.FDL.FieldNames.OutwardTaxi, "1");
            if (fdl.OutwardAircraft)
                fields.Add(ApplicationSettings.FDL.FieldNames.OutwardAircraft, "1");

            if (fdl.ReturnCar)
                fields.Add(ApplicationSettings.FDL.FieldNames.ReturnCar, "1");
            if (fdl.ReturnTaxi)
                fields.Add(ApplicationSettings.FDL.FieldNames.ReturnTaxi, "1");
            if (fdl.ReturnAircraft)
                fields.Add(ApplicationSettings.FDL.FieldNames.ReturnAircraft, "1");

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

            fields.Add(ApplicationSettings.FDL.FieldNames.AssistantFinalTestResult, fdl.ResultNotes != null ? fdl.ResultNotes : string.Empty);
            fields.Add(ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes, fdl.Notes != null ? fdl.Notes : string.Empty);

            return fields;
        }

        private void CompileFDL(FDL fdl, string fileName)
        {
            string source = ApplicationSettings.Directories.FDL + fdl.FileName;
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(source), new PdfWriter(fileName));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();
                
                foreach (KeyValuePair<string, string> entry in GetAcroFormFields(fdl))
                {
                    if(fields.ContainsKey(entry.Key))
                        fields[entry.Key].SetValue(entry.Value);
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
            finally
            {
                pdfDoc?.Close();
            }
        }

        private void CompileXFDF(FDL fdl, string FDLfileName, string FDFFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

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

            foreach (KeyValuePair<string, string> entry in GetAcroFormFields(fdl))
            {
                if (entry.Value == string.Empty)
                    continue;

                XmlNode fieldNode = xmlDoc.CreateElement("field", xfdfNode.NamespaceURI);
                XmlNode valueNode = xmlDoc.CreateElement("value", xfdfNode.NamespaceURI);

                XmlAttribute name = xmlDoc.CreateAttribute("name");
                name.Value = entry.Key;

                fieldNode.Attributes.Append(name);
                valueNode.InnerText = entry.Value;

                fieldNode.AppendChild(valueNode);
                fieldsNode.AppendChild(fieldNode);
            }
            
            xmlDoc.Save(FDFFileName);
        }

        public bool SendToSAP(FDL fdl)
        {
            if (fdl == null)
                return false;

            using (new WaitCursor())
            {
                string filePath = Path.GetTempPath() + fdl.FileName;

                CompileFDL(fdl, filePath);

                EmailMessageDTO message = new EmailMessageDTO();
                message.Subject = $"FDL {fdl.Id} - Factory {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")} - Order {fdl.Order}";                
                message.Importance = Importance.High;
                message.ToRecipients.Add(ApplicationSettings.EmailRecipients.FDLSystem);
                message.CcRecipients.Add(ApplicationSettings.EmailRecipients.HR);
                message.Attachments.Add(filePath);

                exchangeProvider.SendEmail(message);

                fdl.EStatus = EFDLStatus.Waiting; //TODO aggiornare lo stato sull'invio riuscito
                return true;
            }
        }

        public bool SendCancellationRequest(FDL fdl)
        {
            if (fdl == null)
                return false;

            using (new WaitCursor())
            {
                EmailMessageDTO message = new EmailMessageDTO();
                message.Subject = $"Cancellation Request for FDL {fdl.Id}";
                message.Body = $@"Please, cancel the following FDL because it's not applicable.<br>
                                  <br>
                                  FDL: <b>{fdl.Id}</b><br>
                                  Factory: {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")}<br>
                                  Order: {fdl.Order}<br>
                                  <br>
                                  Thank you";
                message.Importance = Importance.High;

                foreach(string address in UserSettings.Email.Recipients.FDLCancelRequest)
                    message.ToRecipients.Add(address);

                exchangeProvider.SendEmail(message);

                fdl.EStatus = EFDLStatus.Cancelled; //TODO aggiornare lo stato sull'invio riuscito
                return true;
            }
        }

        public bool SaveFDL(FDL fdl, string filePath)
        {
            if (fdl == null || filePath == string.Empty)
                return false;

            using (new WaitCursor())
            {
                CompileFDL(fdl, filePath);
                return true;
            }
        }

        public bool SaveXFDF(FDL fdl, string filePath)
        {
            if (fdl == null || filePath == string.Empty)
                return false;

            using (new WaitCursor())
            {
                File.Copy(ApplicationSettings.Directories.FDL + fdl.FileName, Path.GetDirectoryName(filePath) + "\\" + fdl.FileName, true);
                CompileXFDF(fdl, Path.GetDirectoryName(filePath) + "\\" + fdl.FileName, filePath);
                return true;
            }
        }
        
        private void ProcessMessage(EmailMessage message)
        {   
            EMessageType type = GetMessageType(message.Subject);
            string fdlNumber = GetFDLNumber(message, type);
            
            switch (type)
            {
                case EMessageType.FDL_Accepted:
                    using (DBEntities db = new DBEntities())
                    {
                        FDL accepted = db.FDLs.SingleOrDefault(f => f.Id == fdlNumber);
                        if (accepted != null && accepted.EStatus != EFDLStatus.Accepted)
                        {
                            accepted.EStatus = EFDLStatus.Accepted;
                            accepted.LastError = null;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<FDL>(this, accepted));
                        }
                    }
                    break;
                case EMessageType.FDL_Rejected:
                    using (DBEntities db = new DBEntities())
                    {
                        FDL rejected = db.FDLs.SingleOrDefault(f => f.Id == fdlNumber);
                        if (rejected != null && rejected.EStatus != EFDLStatus.Rejected && rejected.EStatus != EFDLStatus.Accepted)
                        {
                            rejected.EStatus = EFDLStatus.Rejected;
                            rejected.LastError = message.Body?.Text;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<FDL>(this, rejected));
                        }
                    }
                    break;
                case EMessageType.EA_Rejected:
                case EMessageType.EA_RejectedResubmission:
                    using (DBEntities db = new DBEntities())
                    {
                        //TODO: differenziare la nota spese R da R1
                        ExpenseAccount expenseAccount = db.ExpenseAccounts.SingleOrDefault(ea => ea.FDL == fdlNumber);
                        if (expenseAccount != null && expenseAccount.EStatus != EFDLStatus.Rejected && expenseAccount.EStatus != EFDLStatus.Accepted)
                        {
                            expenseAccount.EStatus = EFDLStatus.Rejected;
                            expenseAccount.LastError = message.Body?.Text;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<ExpenseAccount>(this, expenseAccount));
                        }
                    }
                    break;
                case EMessageType.FDL_EA_New:
                    if(message.HasAttachments)
                    {
                        foreach (Attachment attachment in message.Attachments)
                        {
                            if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                                continue;

                            FileAttachment fileAttachment = attachment as FileAttachment;

                            switch (GetAttachmentType(attachment.Name))
                            {
                                case EAttachmentType.FDL:
                                    if (!File.Exists(ApplicationSettings.Directories.FDL + fileAttachment.Name))
                                        fileAttachment.Load(ApplicationSettings.Directories.FDL + fileAttachment.Name);

                                    ImportFDLFromFile(ApplicationSettings.Directories.FDL + fileAttachment.Name, true, true);
                                    break;
                                case EAttachmentType.ExpenseAccount:
                                    if (!File.Exists(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name))
                                        fileAttachment.Load(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name);

                                    ImportEAFromFile(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name, true, true);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private EMessageType GetMessageType(string subject)
        {
            if (subject.Contains(ApplicationSettings.FDL.FDL_Accepted))
                return EMessageType.FDL_Accepted;
            else if (subject.Contains(ApplicationSettings.FDL.FDL_Rejected))
                return EMessageType.FDL_Rejected;
            else if (subject.Contains(ApplicationSettings.FDL.EA_Rejected))
                return EMessageType.EA_Rejected;
            else if (subject.Contains(ApplicationSettings.FDL.EA_RejectedResubmission))
                return EMessageType.EA_RejectedResubmission;
            else if (subject.Contains(ApplicationSettings.FDL.Reminder))
                return EMessageType.Reminder;
            else if (GetAttachmentType(subject) == EAttachmentType.FDL)
                return EMessageType.FDL_EA_New;
            else
                return EMessageType.Unknown;
        }

        private EAttachmentType GetAttachmentType(string filename)
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
                            return EAttachmentType.ExpenseAccount;
                        else if (CID.All(char.IsDigit) &&
                                 WeekNr.All(char.IsDigit) && Enumerable.Range(1, 52).Contains(int.Parse(WeekNr)) &&
                                 Month.All(char.IsDigit) && Enumerable.Range(1, 12).Contains(int.Parse(Month)) &&
                                 Year.All(char.IsDigit) && Enumerable.Range(ApplicationSettings.Timesheets.MinYear, ApplicationSettings.Timesheets.MaxYear).Contains(int.Parse(Year)))
                            return EAttachmentType.FDL;
                    }
                }
            }
            catch { }
            
            return EAttachmentType.Unknown;
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
                        if (!(attachment is FileAttachment) || attachment.ContentType != ApplicationSettings.FDL.MIMEType)
                            continue;

                        EAttachmentType attType = GetAttachmentType(attachment.Name);

                        if (attType == EAttachmentType.Unknown)
                            continue;

                        words = Path.GetFileNameWithoutExtension(attachment.Name).Split(' ');

                        if (attType == EAttachmentType.FDL)
                            FDL = $"{words[words.Length - 1]}/{words[0]}";
                        else if (attType == EAttachmentType.ExpenseAccount)
                            FDL = $"{words[words.Length - 2]}/{words[0]}";
                        break;
                    }
                }

                if (FDL == string.Empty)
                {
                    // NB In case of missing attachments, there might be an inconrrect result if you recive the message regarding an FDL of the previous year in the new year.
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
                        case EMessageType.EA_Rejected:
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
    
    public enum EAttachmentType
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
        EA_Rejected,
        EA_RejectedResubmission,
        FDL_EA_New,
        Reminder
    }

    public enum EExchangeStatus
    {
        Offline,
        Connecting,
        Online,
        Reconnecting,
        Error
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
