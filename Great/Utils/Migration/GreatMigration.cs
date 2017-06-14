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
    public class GreatMigration
    {
        #region Events
        public delegate void OperationCompletedHandler(object source, EventImportArgs args);
        public event OperationCompletedHandler OnOperationCompleted;
        #endregion

        #region Constants

        public const string sGreatDefaultInstallationFolder = @"C:\Program Files (x86)\GREAT";
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

        public bool Completed { get; internal set; }
        #endregion

        #region Fields

        //Access database fields
        private OleDbConnection connection;
        private OleDbCommand command;
        private OleDbDataAdapter adapter;
        private DBEntities aEntities = new DBEntities();
        private Thread thrd;

        //Data from access database
        private DataTable dtHours = new DataTable();
        private DataTable dtPlants = new DataTable();
        private DataTable dtCars = new DataTable();
        private DataTable dtExpenseReview = new DataTable();
        private DataTable dtConfiguration = new DataTable();
        #endregion

        public GreatMigration()
        {
            aEntities.Database.Connection.ConnectionString = string.Format(sSqliteConnectionString, ApplicationSettings.Database.DBFileName);
        }

        public void StartMigration(string greatPath)
        {
            Completed = false;
            if (Directory.Exists(greatPath))
            {
                _sourceDatabase = GetGreatDatabaseFile(File.ReadAllLines(Path.Combine(greatPath, sGreatIniFilePath))
                                                                                                                    .Where(x => x.Contains("Dir Backup"))
                                                                                                                    .FirstOrDefault()
                                                                                                                    .Split('=')[1]);

                if (_sourceDatabase != null)
                {
                    GetDataTables();

                    _sourceFdlPath = dtConfiguration.Rows[3].ItemArray[1].ToString();
                    _sourceAccountPath = dtConfiguration.Rows[4].ItemArray[1].ToString();

                    if (Directory.Exists(_sourceFdlPath) && Directory.Exists(_sourceAccountPath))
                    {
                        thrd = new Thread(new ThreadStart(MigrationThread));
                        thrd.Start();
                    }
                    else OnCompleted(new EventImportArgs("old pdf path not found"));
                }
                else OnCompleted(new EventImportArgs("Database not found!"));
            }
            else OnCompleted(new EventImportArgs("Great Path not existing"));
        }

        private void MigrationThread()
        {
            // CleanDBTables();
            OnCompleted(new EventImportArgs("Importing PDF files to DB"));
            CompileFdlTable();
            OnCompleted(new EventImportArgs("Importing Hours"));
            CompileHourTable();
            Completed = true;
            OnCompleted(new EventImportArgs("Operation Completed"));
        }

        private bool GetDataTables()
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

        private bool CompileHourTable()
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

                    if (duplicatedEntities.Count() == 0)
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

        private bool CompileFdlTable()
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
                        fdl.Result = Convert.ToInt64(fields[ApplicationSettings.FDL.FieldNames.Result].GetValueAsString());
                        fdl.ResultNotes = fields[ApplicationSettings.FDL.FieldNames.Result].GetValueAsString();
                        fdl.Notes = fields[ApplicationSettings.FDL.FieldNames.SoftwareVersionsOtherNotes].GetValueAsString();
                        fdl.WeekNr = Convert.ToInt64(s.Substring(s.Length - 14).Substring(0, 2));
                        fdl.Status = 2;
                        fdl.LastError = "";
                        fdl.ReturnCar = Convert.ToBoolean(fields[ApplicationSettings.FDL.FieldNames.ReturnCar].GetValue());
                        fdl.ReturnTaxi = Convert.ToBoolean(fields[ApplicationSettings.FDL.FieldNames.ReturnTaxi].GetValue());
                        fdl.ReturnAircraft = Convert.ToBoolean(fields[ApplicationSettings.FDL.FieldNames.ReturnAircraft].GetValue());
                        fdl.OutwardAircraft = Convert.ToBoolean(fields[ApplicationSettings.FDL.FieldNames.OutwardAircraft].GetValue());
                        fdl.OutwardCar = Convert.ToBoolean(fields[ApplicationSettings.FDL.FieldNames.OutwardCar].GetValue());
                        fdl.OutwardTaxi = Convert.ToBoolean(fields[ApplicationSettings.FDL.FieldNames.OutwardTaxi].GetValue());

                        pdfDoc.Close();
                        aEntities.FDLs.Add(fdl);

                        File.Copy(s, Path.Combine(ApplicationSettings.Directories.FDL, new FileInfo(s).Name), true);
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

            var childUri = new DirectoryInfo(path).Parent;

            if (Environment.OSVersion.Version.Major < 6)
                return (Path.Combine(path, "Archivio.mdb"));
            else
                return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"VirtualStore\", childUri.Parent.Name), Path.Combine(childUri.Name, @"DB\Archivio.mdb"));
        }

        private string[] GetFileList(string sDir)
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
        #endregion

        #region Events
        protected void OnCompleted(EventImportArgs e)
        {
            OnOperationCompleted?.Invoke(this, e);
        }
        #endregion
    }

    public class EventImportArgs : EventArgs
    {
        public EventImportArgs(string result)
        {
            Result = result;
        }
        public string Result { get; set; }
    }
}
