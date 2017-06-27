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
using Microsoft.Practices.ServiceLocation;
using System.Data.Entity.Migrations;

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

                    _sourceFdlPath = dtConfiguration.AsEnumerable().Where(x => x.Field<byte>("Dbf_File") == 5).Select(x => x.Field<string>("Dbf_Path")).FirstOrDefault();                    
                    _sourceAccountPath = dtConfiguration.AsEnumerable().Where(x => x.Field<byte>("Dbf_File") == 8).Select(x => x.Field<string>("Dbf_Path")).FirstOrDefault();

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

            using (DBEntities db = new DBEntities())
            {
                Timesheet t = null;
                try
                {
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

                        db.Days.AddOrUpdate(d);
                        
                        t = new Timesheet();
                        t.Timestamp = r.Field<DateTime>("Dbf_Data").ToUnixTimestamp();

                        var duplicatedEntities = db.Timesheets.Where(x => x.Timestamp == t.Timestamp);

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

                                db.Timesheets.Add(t);
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


                                    db.Timesheets.Add(t);
                                }

                                //Add details for second FDL
                                if (!(string.IsNullOrEmpty(r.Field<string>("Dbf_SecondoFoglio")) | string.IsNullOrWhiteSpace(r.Field<string>("Dbf_SecondoFoglio"))))
                                {
                                    t.FDL = r.Field<string>("Dbf_Foglio");
                                    string[] parts = t.FDL.Split('/');

                                    for (int i = 0; i < parts.Length; i++)
                                        parts[i] = parts[i].Trim();

                                    t.FDL = $"{parts[1]}/{parts[0]}";

                                    db.Timesheets.Add(t);
                                }
                            }
                        }                        
                    }

                    db.SaveChanges();
                    db.Database.Connection.Close();
                }
                catch (Exception ex)
                {   
                    result = false;
                }
            }

            return result;
        }

        private bool CompileFdlTable()
        {
            bool result = false;
            
            try
            {
                foreach (string s in GetFileList(_sourceFdlPath))
                {
                    try
                    {
                        FDLManager manager = ServiceLocator.Current.GetInstance<FDLManager>();

                        if(manager.ImportFDLFromFile(s, false, true, true, true) != null)
                            File.Copy(s, Path.Combine(ApplicationSettings.Directories.FDL, new FileInfo(s).Name), true);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        //If here, a problem with fdl file occured (check filename, and that FDL is not a virtual printed PDF)
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        #region Auxiliar methods
        private void CleanDBTables()
        {
            using (DBEntities db = new DBEntities())
            {
                db.Timesheets.RemoveRange(db.Timesheets);
                db.FDLs.RemoveRange(db.FDLs);
                db.Days.RemoveRange(db.Days);

                db.SaveChanges();
            }

            foreach (string s in GetFileList(_destinationFdlPath))
                File.Delete(s);
        }

        private void InitializeDataAccess()
        {
            try
            {
                connection = new OleDbConnection(string.Format(sAccessConnectionstring, _sourceDatabase));
                connection.Open();
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private string GetGreatDatabaseFile(string path)
        {
            var childUri = new DirectoryInfo(path).Parent;
            string virtualStorePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"VirtualStore\", childUri.Parent.Name), Path.Combine(childUri.Name, @"DB\Archivio.mdb"));
            
            if (File.Exists(virtualStorePath))
                return virtualStorePath;
            else
                return (Path.Combine(path, "Archivio.mdb"));
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
