using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.OleDb;
using System.Data;
using Great.Models;
using Great.Utils.Extensions;
using System.IO;
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;
using System.Threading;
using System.Diagnostics;

namespace Great.Utils
{
    public class EventImportArgs : EventArgs
    {
        public EventImportArgs(string result)
        {
            Result = result;
        }
        public string Result { get; set; }
    }
    class GreatMigration
    {
        #region Events
        public delegate void OperationCompletedHandler(object source, EventImportArgs args);
        public event OperationCompletedHandler OnOperationCompleted;
        #endregion

        #region Constants

        const string sAccessConnectionstring = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";
        const string sSqliteConnectionString = @"data source={0}";
        const string sGreatIniFilePath = @"Ini\Setting.ini";
        string[] sPathToCheck = new string[] { @"C:\Program Files" };

        #endregion

        #region Properties
        public string _sourceDatabase { get; private set; }
        public string _sourceFdlPath { get; private set; }
        public string _sourceAccountPath { get; private set; }
        public string _destinationFdlPath { get; private set; }
        public string _destinationAccountPath { get; private set; }
        #endregion

        #region Fields

        //Access database fields
        OleDbConnection connection;
        OleDbCommand command;
        OleDbDataAdapter adapter;
        DBEntities aEntities = new DBEntities();
        Thread thrd;

        //Data from access database
        DataTable dtHours = new DataTable();
        DataTable dtPlants = new DataTable();
        DataTable dtCars = new DataTable();
        DataTable dtExpenseReview = new DataTable();
        DataTable dtConfiguration = new DataTable();
        #endregion

        public GreatMigration(string greatPath)
        {
            aEntities.Database.Connection.ConnectionString = string.Format(sSqliteConnectionString, ApplicationSettings.Database.DBFileName);

            _sourceDatabase = GetGreatDatabaseFile(File.ReadAllLines(Path.Combine(greatPath, sGreatIniFilePath))
                                                                                                                .Where(x => x.Contains("Dir Backup"))
                                                                                                                .FirstOrDefault()
                                                                                                                .Split('=')[1]);

            if (_sourceDatabase != null)
            {
                GetDataTables();

                _sourceFdlPath = dtConfiguration.Rows[3].ItemArray[1].ToString();
                _sourceAccountPath = dtConfiguration.Rows[4].ItemArray[1].ToString();

                thrd = new Thread(new ThreadStart(executeMigration));
                thrd.Start();
            }
            else OnCompleted(new EventImportArgs("Database not found!"));


        }

        void executeMigration()
        {
            CleanDBTables();
            OnCompleted(new EventImportArgs("Importing PDF files to DB"));
            CompileFdlTable();
            OnCompleted(new EventImportArgs("Importing Hours"));
            CompileHourTable();
            OnCompleted(new EventImportArgs("Operation Completed"));
        }

        public bool GetDataTables()
        {
            bool result = false;

            try
            {
                InitializeDataAccess();

                command = new OleDbCommand("SELECT * FROM dbt_Ore", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtHours);

                command = new OleDbCommand("SELECT * FROM dbt_Auto", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtCars);

                command = new OleDbCommand("SELECT * FROM dbt_NotaSpese", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtExpenseReview);

                command = new OleDbCommand("SELECT * FROM dbt_Stabilimenti", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtPlants);

                command = new OleDbCommand("SELECT * FROM dbt_File_Config", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtConfiguration);

                Close();

                result = true;
            }
            catch (Exception ex)
            {

                result = false;
            }

            return result;
        }
        public bool CompileHourTable()
        {
            bool result = false;

            try
            {
                aEntities.Database.Connection.Open();

                //Get enumerable rows fron datatable
                IEnumerable<DataRow> collection = dtHours.Rows.Cast<DataRow>();

                foreach (DataRow r in collection)
                {
                    Day d = new Day();
                    d.Type = r.Field<byte>("dbf_Tipo_Giorno");

                    if (d.Type != 3 && d.Type != 6)
                        d.Type = 0;
                    else if (d.Type == 3)
                        d.Type = 1;
                    else if (d.Type == 6)
                        d.Type = 2;

                    d.Timestamp = r.Field<DateTime>("Dbf_Data").ToUnixTimestamp();

                    Timesheet t = new Timesheet();
                    t.Timestamp = r.Field<DateTime>("Dbf_Data").ToUnixTimestamp();

                    var duplicatedEntities = aEntities.Timesheets.Where(x => x.Timestamp == t.Timestamp);

                    if (duplicatedEntities.Count() > 0)
                    {

                    }
                    else
                    {
                        //Add office hourrs
                        if (
                        r.Field<Int16>("Dbf_Uff_Inizio_AM") != 0 |
                        r.Field<Int16>("Dbf_Uff_Fine_AM") != 0 |
                        r.Field<Int16>("Dbf_Uff_Inizio_PM") != 0 |
                        r.Field<Int16>("Dbf_Uff_Fine_PM") != 0
                      )
                        {

                            t.TravelStartTimeAM = null;
                            t.WorkStartTimeAM = r.Field<Int16>("Dbf_Uff_Inizio_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Inizio_AM")).TotalSeconds : null;
                            t.WorkEndTimeAM = r.Field<Int16>("Dbf_Uff_Fine_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Fine_AM")).TotalSeconds : null;
                            t.TravelEndTimeAM = null;
                            t.TravelStartTimePM = null;
                            t.WorkStartTimePM = r.Field<Int16>("Dbf_Uff_Inizio_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Inizio_PM")).TotalSeconds : null;
                            t.WorkEndTimePM = r.Field<Int16>("Dbf_Uff_Fine_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Fine_PM")).TotalSeconds : null;
                            t.TravelEndTimePM = null;

                            aEntities.Timesheets.Add(t);
                        }

                        //Add FDL hours
                        if (
                            r.Field<Int16>("Dbf_Partenza_AM") != 0 |
                            r.Field<Int16>("Dbf_Trasf_Inizio_AM") != 0 |
                            r.Field<Int16>("Dbf_Trasf_Fine_AM") != 0 |
                            r.Field<Int16>("Dbf_Arrivo_AM") != 0 |
                            r.Field<Int16>("Dbf_Partenza_PM") != 0 |
                            r.Field<Int16>("Dbf_Trasf_Inizio_PM") != 0 |
                            r.Field<Int16>("Dbf_Trasf_Fine_PM") != 0
                          )
                        {
                            //Compile FDL details
                            t.TravelStartTimeAM = r.Field<Int16>("Dbf_Partenza_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Partenza_AM")).TotalSeconds : null;
                            t.WorkStartTimeAM = r.Field<Int16>("Dbf_Trasf_Inizio_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Trasf_Inizio_AM")).TotalSeconds : null;
                            t.WorkEndTimeAM = r.Field<Int16>("Dbf_Trasf_Fine_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Trasf_Fine_AM")).TotalSeconds : null;
                            t.TravelEndTimeAM = r.Field<Int16>("Dbf_Arrivo_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Arrivo_AM")).TotalSeconds : null;
                            t.TravelStartTimePM = r.Field<Int16>("Dbf_Partenza_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Partenza_PM")).TotalSeconds : null;
                            t.WorkStartTimePM = r.Field<Int16>("Dbf_Trasf_Inizio_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Trasf_Inizio_PM")).TotalSeconds : null;
                            t.WorkEndTimePM = r.Field<Int16>("Dbf_Trasf_Fine_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Trasf_Fine_PM")).TotalSeconds : null;
                            t.TravelEndTimePM = r.Field<Int16>("Dbf_Arrivo_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Arrivo_PM")).TotalSeconds : null;

                            //Add details for first FDL
                            if (!(string.IsNullOrEmpty(r.Field<string>("Dbf_Foglio")) | string.IsNullOrWhiteSpace(r.Field<string>("Dbf_Foglio"))))
                            {
                                t.FDL = r.Field<string>("Dbf_Foglio");
                                string[] parts = t.FDL.Split('/');

                                for (int i = 0; i < parts.Length; i++)
                                    parts[i] = parts[i].Trim();

                                t.FDL = $"{parts[1]}/{parts[0]}".Trim();


                                aEntities.Timesheets.Add(t);
                            }

                            //Add details for second FDL
                            if (!(string.IsNullOrEmpty(r.Field<string>("Dbf_SecondoFoglio")) | string.IsNullOrWhiteSpace(r.Field<string>("Dbf_SecondoFoglio"))))
                            {
                                t.FDL = r.Field<string>("Dbf_Foglio");
                                string[] parts = t.FDL.Split('/');

                                for (int i = 0; i < parts.Length; i++)
                                    parts[i] = parts[i].Trim();

                                t.FDL = $"{parts[1]}/{parts[0]}";

                                aEntities.Timesheets.Add(t);

                            }
                        }
                    }

                    if (aEntities.Days.Where(x => x.Timestamp == d.Timestamp).Count() == 0) aEntities.Days.Add(d);
                }

                aEntities.SaveChanges();
                aEntities.Database.Connection.Close();
            }

            catch (Exception ex)
            {
                aEntities.Database.Connection.Close();
                result = false;
            }

            return result;
        }
        public bool CompileFdlTable()
        {
            bool result = false;

            foreach (var entity in aEntities.FDLs)
                aEntities.FDLs.Remove(entity);
            try
            {
                aEntities.Database.Connection.Open();

                foreach (string s in GetFileList(_sourceFdlPath))
                {
                    PdfDocument pdfDoc;
                    FDL fdl = new FDL();
                    try
                    {
                        pdfDoc = new PdfDocument(new PdfReader(s));
                        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                        IDictionary<string, PdfFormField> fields = form.GetFormFields();

                        fdl.Id = fields[ApplicationSettings.FDL.FieldNames.FDLNumber].GetValueAsString();
                        fdl.Order = fields[ApplicationSettings.FDL.FieldNames.Order].GetValueAsString();
                        fdl.FileName = s;
                        fdl.IsExtra = fields[ApplicationSettings.FDL.FieldNames.OrderType].GetValueAsString().Contains(ApplicationSettings.FDL.FDL_Extra);
                        fdl.Factory = 0;//??
                        fdl.PerformanceDescription = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescription].GetValueAsString();
                        fdl.PerformanceDescriptionDetails = fields[ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails].GetValueAsString();
                        fdl.Result = 1;
                        fdl.ResultNotes = fields[ApplicationSettings.FDL.FieldNames.Result].GetValueAsString();
                        fdl.Notes = fields[ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes].GetValueAsString();
                        fdl.WeekNr = Convert.ToInt64(s.Substring(s.Length - 14).Substring(0, 2));
                        fdl.Status = 2;
                        fdl.LastError = "";
                        fdl.ReturnCar = false;
                        fdl.ReturnTaxi = false;
                        fdl.ReturnAircraft = false;
                        fdl.OutwardAircraft = false;
                        fdl.OutwardCar = false;
                        fdl.OutwardTaxi = false;

                        aEntities.FDLs.Add(fdl);
                        CompileFDL(fdl, s, Path.Combine(ApplicationSettings.Directories.FDL, new FileInfo(s).Name));
                        

                    }
                    catch (Exception ex)
                    {
                        //If here, a problem with fdl file occured (check filename, and that FDL is not a virtual printed PDF)

                    }

                }

                aEntities.SaveChanges();

                result = true;

                aEntities.Database.Connection.Close();

            }


            catch (Exception ex)
            {
                aEntities.Database.Connection.Close();
                result = false;
            }

            return result;
        }


        #region Auxiliar methods

        private void CleanDBTables()
        {
            foreach (Timesheet t in aEntities.Timesheets)
                aEntities.Timesheets.Remove(t);
            foreach (FDL f in aEntities.FDLs)
                aEntities.FDLs.Remove(f);
            foreach (Day d in aEntities.Days)
                aEntities.Days.Remove(d);
            foreach (string s in GetFileList(_destinationFdlPath))
                File.Delete(s);

            aEntities.SaveChanges();
        }
        private void InitializeDataAccess()
        {
            try
            {
                connection = new OleDbConnection(string.Format(sAccessConnectionstring, _sourceDatabase));
                connection.Open();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private string GetGreatDatabaseFile(string path)
        {
            foreach (string s in sPathToCheck)
            {
                var parentUri = new Uri(s);
                var childUri = new DirectoryInfo(path).Parent;
                while (childUri != null)
                {
                    if (new Uri(childUri.FullName) == parentUri & Environment.OSVersion.Version.Major >= 6)
                        return Path.Combine(path.Replace(childUri.FullName, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"VirtualStore\", childUri.Name)), "Archivio.mdb");

                    childUri = childUri.Parent;
                }
                return Path.Combine(path, "Archivio.mdb");

            }
            return null;

        }

        string[] GetFileList(string sDir)
        {
            List<string> temp = new List<string>();
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, "*.pdf"))
                    {
                        temp.Add(f);
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
            return temp.ToArray();
        }

        public void Close()
        {
            connection.Dispose();
        }

        private void CompileFDL(FDL fdl, string source, string destination)
        {
            PdfDocument pdfDoc = null;

            try
            {
                pdfDoc = new PdfDocument(new PdfReader(source), new PdfWriter(destination));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();

                Timesheet timesheet = null;

                string monday = fields[ApplicationSettings.FDL.FieldNames.Mon_Date].GetValueAsString();
                if (monday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(monday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_WorkEndTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Mon_TravelEndTimePM].SetValue(string.Empty);
                    }
                }

                string tuesday = fields[ApplicationSettings.FDL.FieldNames.Tue_Date].GetValueAsString();
                if (tuesday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(tuesday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_WorkEndTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Tue_TravelEndTimePM].SetValue(string.Empty);
                    }
                }

                string wednesday = fields[ApplicationSettings.FDL.FieldNames.Wed_Date].GetValueAsString();
                if (wednesday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(wednesday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_WorkEndTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Wed_TravelEndTimePM].SetValue(string.Empty);
                    }
                }

                string thursday = fields[ApplicationSettings.FDL.FieldNames.Thu_Date].GetValueAsString();
                if (thursday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(thursday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_WorkEndTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Thu_TravelEndTimePM].SetValue(string.Empty);
                    }
                }

                string friday = fields[ApplicationSettings.FDL.FieldNames.Fri_Date].GetValueAsString();
                if (friday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(friday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_WorkEndTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Fri_TravelEndTimePM].SetValue(string.Empty);
                    }
                }

                string saturday = fields[ApplicationSettings.FDL.FieldNames.Sat_Date].GetValueAsString();
                if (saturday != string.Empty)
                {
                    timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(saturday));

                    if (timesheet != null)
                    {
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkStartTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelEndTimeAM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkStartTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_WorkEndTimePM].SetValue(string.Empty);
                        fields[ApplicationSettings.FDL.FieldNames.Sat_TravelEndTimePM].SetValue(string.Empty);
                    }

                    string sunday = fields[ApplicationSettings.FDL.FieldNames.Sun_Date].GetValueAsString();
                    if (sunday != string.Empty)
                    {
                        timesheet = fdl.Timesheets.SingleOrDefault(t => t.Date == DateTime.Parse(sunday));

                        if (timesheet != null)
                        {
                            fields[ApplicationSettings.FDL.FieldNames.Sun_TravelStartTimeAM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_WorkStartTimeAM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_WorkEndTimeAM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_TravelEndTimeAM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_TravelStartTimePM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_WorkStartTimePM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_WorkEndTimePM].SetValue(string.Empty);
                            fields[ApplicationSettings.FDL.FieldNames.Sun_TravelEndTimePM].SetValue(string.Empty);
                        }
                    }

                    //TODO: pensare a come compilare i campi delle auto, se farlo in automatico oppure se farle selezionare dall'utente
                    //fields[ApplicationSettings.FDL.FieldNames.Cars1]
                    //fields[ApplicationSettings.FDL.FieldNames.Cars2]

                    fields[ApplicationSettings.FDL.FieldNames.OutwardCar].SetValue("0");
                    fields[ApplicationSettings.FDL.FieldNames.OutwardTaxi].SetValue("0");
                    fields[ApplicationSettings.FDL.FieldNames.OutwardAircraft].SetValue("0");
                    fields[ApplicationSettings.FDL.FieldNames.ReturnCar].SetValue("0");
                    fields[ApplicationSettings.FDL.FieldNames.ReturnTaxi].SetValue("0");
                    fields[ApplicationSettings.FDL.FieldNames.ReturnAircraft].SetValue("0");

                    fields[ApplicationSettings.FDL.FieldNames.PerformanceDescription].SetValue(string.Empty);
                    fields[ApplicationSettings.FDL.FieldNames.PerformanceDescriptionDetails].SetValue(string.Empty);
                    fields[ApplicationSettings.FDL.FieldNames.Result].SetValue(string.Empty);
                    fields[ApplicationSettings.FDL.FieldNames.AssistantFinalTestResult].SetValue(string.Empty);
                    fields[ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes].SetValue(string.Empty);
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
        #endregion

        #region Events

        protected void OnCompleted(EventImportArgs e)
        {
            if (OnOperationCompleted != null)
                OnOperationCompleted(this, e);
        }
        #endregion
    }

}
