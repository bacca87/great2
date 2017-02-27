using Great.Utils;
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
using System.Threading;

namespace Great.Models
{
    public class FDLManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private ExchangeService notificationsService { get; set; }
        private StreamingSubscription streamingSubscription { get; set; }
        private StreamingSubscriptionConnection connection { get; set; }

        private Thread subscribeThread;
        private Thread syncThread;
        
        public EExchangeStatus ExchangeStatus { get; internal set; }
        
        public FDLManager()
        {
            StartBackgroundOperations();
        }

        private FDL GetFDLFromFile(string filePath)
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
                
                string mon = fields[ApplicationSettings.FDL.FieldNames.Mon_Date].GetValueAsString();
                string tue = fields[ApplicationSettings.FDL.FieldNames.Tue_Date].GetValueAsString();
                string wed = fields[ApplicationSettings.FDL.FieldNames.Wed_Date].GetValueAsString();
                string thu = fields[ApplicationSettings.FDL.FieldNames.Thu_Date].GetValueAsString();
                string fri = fields[ApplicationSettings.FDL.FieldNames.Fri_Date].GetValueAsString();
                string sat = fields[ApplicationSettings.FDL.FieldNames.Sat_Date].GetValueAsString();
                string sun = fields[ApplicationSettings.FDL.FieldNames.Sun_Date].GetValueAsString();

                if (mon != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(mon));
                else if (tue != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(tue));
                else if (wed != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(wed));
                else if (thu != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(thu));
                else if (fri != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(fri));
                else if (sat != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(sat));
                else if (sun != string.Empty)
                    fdl.WeekNr = DateTimeHelper.WeekNr(DateTime.Parse(sun));
                else
                    throw new InvalidOperationException("Impossible to retrieve the week number.");
            }
            catch
            {
                fdl = null;
            }
            finally
            {
                pdfDoc?.Close();
            }

            return fdl;
        }

        private void CompileFDL(FDL fdl)
        {
            string file = ApplicationSettings.Directories.FDL + fdl.FileName;
            string tempFile = Path.GetTempPath() + fdl.FileName;
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(file), new PdfWriter(tempFile));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();
                
                Timesheet timesheet = null;
                
                string monday = fields[ApplicationSettings.FDL.FieldNames.Mon_Date].GetValueAsString();
                if (monday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(monday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                string tuesday = fields[ApplicationSettings.FDL.FieldNames.Tue_Date].GetValueAsString();
                if (tuesday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(tuesday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }

                string wednesday = fields[ApplicationSettings.FDL.FieldNames.Wed_Date].GetValueAsString();
                if (wednesday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(wednesday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }

                string thursday = fields[ApplicationSettings.FDL.FieldNames.Thu_Date].GetValueAsString();
                if (thursday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(thursday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }

                string friday = fields[ApplicationSettings.FDL.FieldNames.Fri_Date].GetValueAsString();
                if (friday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(friday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }

                string saturday = fields[ApplicationSettings.FDL.FieldNames.Sat_Date].GetValueAsString();
                if (saturday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(saturday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }

                string sunday = fields[ApplicationSettings.FDL.FieldNames.Sun_Date].GetValueAsString();
                if (sunday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(sunday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Sun_TravelStartTimeAM].SetValue(timesheet.TravelStartTimeAM_t.HasValue ? timesheet.TravelStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_WorkStartTimeAM].SetValue(timesheet.WorkStartTimeAM_t.HasValue ? timesheet.WorkStartTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_WorkEndTimeAM].SetValue(timesheet.WorkEndTimeAM_t.HasValue ? timesheet.WorkEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_TravelEndTimeAM].SetValue(timesheet.TravelEndTimeAM_t.HasValue ? timesheet.TravelEndTimeAM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_TravelStartTimePM].SetValue(timesheet.TravelStartTimePM_t.HasValue ? timesheet.TravelStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_WorkStartTimePM].SetValue(timesheet.WorkStartTimePM_t.HasValue ? timesheet.WorkStartTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_WorkEndTimePM].SetValue(timesheet.WorkEndTimePM_t.HasValue ? timesheet.WorkEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sun_TravelEndTimePM].SetValue(timesheet.TravelEndTimePM_t.HasValue ? timesheet.TravelEndTimePM_t.Value.ToString("hh\\:mm") : string.Empty);
                    }
                }

                //TODO: pensare a come compilare i campi delle auto, se farlo in automatico oppure se farle selezionare dall'utente
                //fields[ApplicationSettings.FDL.FieldNames.Cars1]
                //fields[ApplicationSettings.FDL.FieldNames.Cars2]
                
                fields[ApplicationSettings.FDL.FieldNames.OutwardCar].SetValue(fdl.OutwardCar ? "1" : "0");
                fields[ApplicationSettings.FDL.FieldNames.OutwardTaxi].SetValue(fdl.OutwardTaxi ? "1" : "0");
                fields[ApplicationSettings.FDL.FieldNames.OutwardAircraft].SetValue(fdl.OutwardAircraft ? "1" : "0");
                fields[ApplicationSettings.FDL.FieldNames.ReturnCar].SetValue(fdl.ReturnCar ? "1" : "0");
                fields[ApplicationSettings.FDL.FieldNames.ReturnTaxi].SetValue(fdl.ReturnTaxi ? "1" : "0");
                fields[ApplicationSettings.FDL.FieldNames.ReturnAircraft].SetValue(fdl.ReturnAircraft ? "1" : "0");
                
                fields[ApplicationSettings.FDL.FieldNames.PerformanceDescription].SetValue(fdl.PerformanceDescription);
                fields[ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails].SetValue(fdl.PerformanceDescriptionDetails);
                fields[ApplicationSettings.FDL.FieldNames.Result].SetValue(fdl.Result.ToString());
                fields[ApplicationSettings.FDL.FieldNames.AssistantFinalTestResult].SetValue(fdl.ResultNotes);
                fields[ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes].SetValue(fdl.Notes);
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

        public void SendFDL(FDL fdl)
        {
            CompileFDL(fdl);

            EmailMessage message = new EmailMessage(notificationsService);
            message.Subject = $"FDL {fdl.Year + "/" + fdl.Id.ToString().PadLeft(5, '0')} - Factory {(fdl.Factory1 != null ? fdl.Factory1.Name : "Unknown")} - Order {fdl.Order}";
            message.ToRecipients.Add(ApplicationSettings.FDL.EmailAddress);
            message.Attachments.AddFileAttachment(Path.GetTempPath() + fdl.FileName);
            message.SendAndSaveCopy();

            // Delete the temporary FDL file
            File.Delete(Path.GetTempPath() + fdl.FileName);
        }
        
        private void StartBackgroundOperations()
        {
            subscribeThread = new Thread(SubscribeNotifications);
            subscribeThread.Name = "Exchange Subscription Thread";
            subscribeThread.IsBackground = true;
            subscribeThread.Start();

            syncThread = new Thread(ExchangeSync);
            syncThread.Name = "Exchange Sync";
            syncThread.IsBackground = true;
            syncThread.Start();
        }

        private void SyncAll(ExchangeService service)
        {
            DBEntities db = new DBEntities();
            ItemView itemView = new ItemView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly) };
            FolderView folderView = new FolderView(int.MaxValue) { PropertySet = new PropertySet(BasePropertySet.IdOnly), Traversal = FolderTraversal.Deep };

            itemView.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Ascending);

            Directory.CreateDirectory(ApplicationSettings.Directories.FDL);

            foreach (Item item in FindItemsInSubfolders(service, new FolderId(WellKnownFolderName.MsgFolderRoot), "from:" + ApplicationSettings.FDL.EmailAddress, folderView, itemView))
            {
                if (!(item is EmailMessage))
                    continue;

                EmailMessage message = EmailMessage.Bind(service, item.Id);
                ProcessMessage(message, db);
            }

            db.SaveChanges();
        }

        private void ProcessMessage(EmailMessage message, DBEntities db)
        {
            EMessageType type = GetMessageType(message.Subject);
            string fdlNumber = ExtractFDLFromSubject(message.Subject, type);
            
            switch (type)
            {
                case EMessageType.FDL_Accepted:
                    FDL accepted = db.FDLs.SingleOrDefault(f => f.Id.Substring(5) == fdlNumber);
                    if (accepted != null)
                        accepted.Status = (long)EFDLStatus.Accepted;
                    break;
                case EMessageType.FDL_Rejected:
                    FDL rejected = db.FDLs.SingleOrDefault(f => f.Id.Substring(5) == fdlNumber);
                    if (rejected != null)
                    {
                        rejected.Status = (long)EFDLStatus.Rejected;
                        rejected.LastError = message.Body?.Text;
                    }
                    break;
                case EMessageType.EA_Rejected:
                case EMessageType.EA_RejectedResubmission:
                    //TODO: differenziare la nota spese R da R1
                    ExpenseAccount expenseAccount = db.ExpenseAccounts.SingleOrDefault(ea => ea.FDL.Substring(5) == fdlNumber);
                    if (expenseAccount != null)
                    {
                        expenseAccount.Status = (long)EFDLStatus.Rejected;
                        expenseAccount.LastError = message.Body?.Text;
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
                                //TODO: inserire su db i nuovi FDL e Note spese
                                case EAttachmentType.FDL:
                                    if (!File.Exists(ApplicationSettings.Directories.FDL + fileAttachment.Name))
                                        fileAttachment.Load(ApplicationSettings.Directories.FDL + fileAttachment.Name);

                                    FDL fdl = GetFDLFromFile(ApplicationSettings.Directories.FDL + fileAttachment.Name);

                                    if (fdl != null && !db.FDLs.Any(f => f.Id == fdl.Id))
                                        db.FDLs.Add(fdl);
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
        
        private IEnumerable<Item> FindItemsInSubfolders(ExchangeService service, FolderId root, string query, FolderView folderView, ItemView itemView)
        {
            FindFoldersResults foldersResults;
            FindItemsResults<Item> itemsResults;

            do
            {
                foldersResults = service.FindFolders(root, folderView);

                foreach (Folder folder in foldersResults)
                {
                    do
                    {
                        itemsResults = service.FindItems(folder.Id, query, itemView);
                        
                        foreach (Item item in itemsResults)
                            yield return item;

                        if (itemsResults.MoreAvailable)
                            itemView.Offset += itemView.PageSize;

                    } while (itemsResults.MoreAvailable);
                }

                if (foldersResults.MoreAvailable)
                    folderView.Offset += folderView.PageSize;

            } while (foldersResults.MoreAvailable);

            // reset the offset for a new search in current folder
            itemView.Offset = 0;

            do
            {
                itemsResults = service.FindItems(root, query, itemView);

                foreach (Item item in itemsResults)
                    yield return item;

                if (itemsResults.MoreAvailable)
                    itemView.Offset += itemView.PageSize;

            } while (itemsResults.MoreAvailable);
        }
        
        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
                result = true;

            return result;
        }

        #region Threads
        private void SubscribeNotifications()
        {
            bool IsSubscribed = false;

            notificationsService = new ExchangeService();
            notificationsService.TraceEnabled = true;
            notificationsService.TraceFlags = TraceFlags.None;
            
            do
            {
                try
                {
                    notificationsService.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    notificationsService.AutodiscoverUrl(UserSettings.Email.EmailAddress, RedirectionUrlValidationCallback);

                    streamingSubscription = notificationsService.SubscribeToStreamingNotificationsOnAllFolders(EventType.NewMail);

                    connection = new StreamingSubscriptionConnection(notificationsService, 30);
                    connection.AddSubscription(streamingSubscription);
                    connection.OnNotificationEvent += Connection_OnNotificationEvent;
                    connection.OnSubscriptionError += Connection_OnSubscriptionError;
                    connection.OnDisconnect += Connection_OnDisconnect;
                    connection.Open();

                    IsSubscribed = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsSubscribed);
        }

        private void ExchangeSync()
        {
            bool IsSynced = false;

            ExchangeService service = new ExchangeService();
            service.TraceEnabled = true;
            service.TraceFlags = TraceFlags.None;
            
            do
            {
                try
                {
                    service.Credentials = new WebCredentials(UserSettings.Email.EmailAddress, UserSettings.Email.EmailPassword);
                    service.AutodiscoverUrl(UserSettings.Email.EmailAddress, RedirectionUrlValidationCallback);

                    SyncAll(service);

                    IsSynced = true;
                }
                catch { Thread.Sleep(ApplicationSettings.General.WaitForNextConnectionRetry); }
            } while (!IsSynced);
        }
        #endregion

        #region Subscription Events Handling
        private void Connection_OnNotificationEvent(object sender, NotificationEventArgs args)
        {
            DBEntities db = new DBEntities();

            foreach (NotificationEvent e in args.Events)
            {
                var itemEvent = (ItemEvent)e;
                EmailMessage message = EmailMessage.Bind(notificationsService, itemEvent.ItemId);

                switch (e.EventType)
                {
                    case EventType.NewMail:
                        ProcessMessage(message, db);
                        break;

                    default:
                        break;
                }
            }

            db.SaveChanges();
        }

        private void Connection_OnDisconnect(object sender, SubscriptionErrorEventArgs args)
        {
            connection.Open();
            Debugger.Break();
        }

        private void Connection_OnSubscriptionError(object sender, SubscriptionErrorEventArgs args)
        {
            Debugger.Break();
        }
        #endregion
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
        Disconnected,
        Syncing,
        Connected
    }

    public enum EFDLStatus
    {
        New = 0,
        Waiting = 1,
        Accepted = 2,
        Rejected = 3
    }
}
