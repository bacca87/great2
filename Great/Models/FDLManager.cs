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
        }

        private FDL CreateNewFDLFromFile(string filePath)
        {   
            FDL fdl = new FDL();
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(filePath));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();
                
                fdl.Id = fields[ApplicationSettings.FDL.FieldNames.FDLNumber].GetValueAsString();                
                fdl.Order = fields[ApplicationSettings.FDL.FieldNames.Order].GetValueAsString();
                fdl.FileName = Path.GetFileName(filePath);
                fdl.IsExtra = fields[ApplicationSettings.FDL.FieldNames.OrderType].GetValueAsString().Contains(ApplicationSettings.FDL.FDL_Extra);
                fdl.Result = 0; 

                string[] days = new string[]
                {
                    fields[ApplicationSettings.FDL.FieldNames.Mon_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Tue_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Wed_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Thu_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Fri_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Sat_Date].GetValueAsString(),
                    fields[ApplicationSettings.FDL.FieldNames.Sun_Date].GetValueAsString()
                };

                foreach(string day in days)
                {
                    if (day != string.Empty)
                    {
                        fdl.WeekNr = DateTime.Parse(day).WeekNr();
                        break;
                    }
                }

                if (fdl.WeekNr == 0)
                    throw new InvalidOperationException("Impossible to retrieve the week number.");

                using (DBEntities db = new DBEntities())
                {
                    if (!db.FDLs.Any(f => f.Id == fdl.Id))
                    {
                        // Automatic factories creation
                        string customer = fields[ApplicationSettings.FDL.FieldNames.Customer].GetValueAsString();
                        string address = fields[ApplicationSettings.FDL.FieldNames.Address].GetValueAsString();

                        if (address != string.Empty && customer != string.Empty)
                        {
                            //TODO: migliorare riconoscimento stabilimenti e inserire flag per attivazione disattivazione inserimento automatico stabilimenti
                            Factory factory = db.Factories.SingleOrDefault(f => f.Address.ToLower() == address.ToLower());

                            if (factory == null && UserSettings.Advanced.AutoAddFactories)
                            {
                                factory = new Factory()
                                {
                                    Name = customer,
                                    CompanyName = customer,
                                    Address = address,
                                    NotifyAsNew = true
                                };

                                db.Factories.Add(factory);
                                db.SaveChanges();

                                Messenger.Default.Send(new NewItemMessage<Factory>(this, factory));
                            }

                            if(UserSettings.Advanced.AutoAssignFactories)
                                fdl.Factory = factory.Id;
                        }

                        fdl.NotifyAsNew = true;

                        db.FDLs.Add(fdl);
                        db.SaveChanges();

                        Messenger.Default.Send(new NewItemMessage<FDL>(this, fdl));
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

            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Monday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Mon_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }
          
            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Tuesday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Tue_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }
          
            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Wednesday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Wed_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }
          
            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Thursday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Thu_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }
           
            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Friday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Fri_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }
                
            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Saturday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sat_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
            }
          
            timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date.DayOfWeek == DayOfWeek.Sunday);

            if (timesheet != null)
            {
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_TravelStartTimeAM, timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_WorkStartTimeAM, timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_WorkEndTimeAM, timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_TravelEndTimeAM, timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_TravelStartTimePM, timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_WorkStartTimePM, timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_WorkEndTimePM, timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                fields.Add(ApplicationSettings.FDL.FieldNames.Sun_TravelEndTimePM, timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
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
            string fdlNumber = ExtractFDLFromSubject(message.Subject, type);
            
            switch (type)
            {
                case EMessageType.FDL_Accepted:
                    using (DBEntities db = new DBEntities())
                    {
                        FDL accepted = db.FDLs.SingleOrDefault(f => f.Id.Substring(5) == fdlNumber);
                        if (accepted != null && accepted.EStatus != EFDLStatus.Accepted)
                        {
                            accepted.EStatus = EFDLStatus.Accepted;
                            db.SaveChanges();
                            Messenger.Default.Send(new ItemChangedMessage<FDL>(this, accepted));
                        }
                    }
                    break;
                case EMessageType.FDL_Rejected:
                    using (DBEntities db = new DBEntities())
                    {
                        FDL rejected = db.FDLs.SingleOrDefault(f => f.Id.Substring(5) == fdlNumber);
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
                        ExpenseAccount expenseAccount = db.ExpenseAccounts.SingleOrDefault(ea => ea.FDL.Substring(5) == fdlNumber);
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

                            switch (GetAttachmentType(Path.GetFileNameWithoutExtension(attachment.Name)))
                            {
                                //TODO: inserire su db Note spese
                                case EAttachmentType.FDL:
                                    if (!File.Exists(ApplicationSettings.Directories.FDL + fileAttachment.Name))
                                        fileAttachment.Load(ApplicationSettings.Directories.FDL + fileAttachment.Name);

                                    CreateNewFDLFromFile(ApplicationSettings.Directories.FDL + fileAttachment.Name);
                                    break;
                                case EAttachmentType.ExpenseAccount1:
                                case EAttachmentType.ExpenseAccount2:
                                    if (!File.Exists(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name))
                                        fileAttachment.Load(ApplicationSettings.Directories.ExpenseAccount + fileAttachment.Name);
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
            else if (GetAttachmentType(subject) == EAttachmentType.FDL)
                return EMessageType.FDL_EA_New;
            else
                return EMessageType.Unknown;
        }

        private EAttachmentType GetAttachmentType(string filename)
        {
            try
            {
                string[] words = filename.Split(' ');

                if (words.Length > 5)
                {
                    string FDL = words[0];
                    string CID = words[words.Length - 4];
                    string WeekNr = words[words.Length - 3];
                    string Month = words[words.Length - 2];
                    string Year = words[words.Length - 1];

                    if (FDL.All(char.IsDigit))
                    {
                        if (words.LastOrDefault().Contains("R1"))
                            return EAttachmentType.ExpenseAccount2;
                        else if (words.LastOrDefault().Contains("R"))
                            return EAttachmentType.ExpenseAccount1;
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
        
        private string ExtractFDLFromSubject(string subject, EMessageType type)
        {
            string FDL = string.Empty;
            string[] words;

            try
            {
                switch(type)
                {
                    case EMessageType.FDL_Accepted:
                    case EMessageType.FDL_Rejected:
                        // INVALID FDL (XXXXX)
                        // FDL RECEIVED (XXXXX)
                        Match match = Regex.Match(subject, @"\(([^)]*)\)");
                        if (match.Success || match.Groups.Count > 0)
                            FDL = match.Groups[1].Value;
                        break;
                    case EMessageType.EA_Rejected:
                        // FDL XXXXX NOTA SPESE RIFIUTATA
                        //  0    1    2     3       4
                        words = subject.Split(' ');
                        if(words.Length > 1)
                            FDL = words[1];
                        break;
                    case EMessageType.EA_RejectedResubmission:
                        // Reinvio nota spese YYYY/XXXXX respinto
                        //    0      1    2       3         4
                        words = subject.Split(' ');
                        if (words.Length > 3)
                        {
                            words = words[3].Split('/');
                            if(words.Length > 1)
                                FDL = words[1];
                        }
                        break;
                    default:
                        break;
                }
            }
            catch { }

            return FDL;
        }
    }

    public enum EAttachmentType
    {
        Unknown,
        FDL,
        ExpenseAccount1,
        ExpenseAccount2
    }

    public enum EMessageType
    {
        Unknown,
        FDL_Accepted,
        FDL_Rejected,
        EA_Rejected,
        EA_RejectedResubmission,
        FDL_EA_New
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
}
