using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.OleDb;
using System.Data;
using Great.Models;
using Great.Utils.Extensions;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Data.Entity.Migrations;
using GalaSoft.MvvmLight.Ioc;
using Great.Models.Database;

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
        private DataTable dtSentFiles = new DataTable();
        #endregion

        #region Temp Cache
        IDictionary<long, long> _factories = new Dictionary<long, long>();
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
            OnCompleted(new EventImportArgs("Importing Factories..."));
            CompileFactoriesTable();
            OnCompleted(new EventImportArgs("Importing PDF files..."));
            CompileFdlTable();
            OnCompleted(new EventImportArgs("Importing Hours..."));
            CompileHourTable();
            Completed = true;
            OnCompleted(new EventImportArgs("Operation Completed!"));
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

                command = new OleDbCommand("SELECT * FROM dbt_Invii", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtSentFiles);

                Close();

                result = true;
            }
            catch (Exception ex)
            {

                result = false;
            }

            return result;
        }

        private bool CompileFactoriesTable()
        {
            bool result = false;

            using (DBArchive db = new DBArchive())
            {
                Timesheet t = null;
                try
                {
                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtPlants.Rows.Cast<DataRow>();

                    foreach (DataRow r in collection)
                    {
                        Factory f = new Factory();
                        f.Name = r.Field<string>("dbf_Stabilimento");
                        f.CompanyName = r.Field<string>("dbf_RagioneSociale");
                        f.Address = r.Field<string>("dbf_Indirizzo");
                        f.IsForfait = r.Field<bool>("dbf_Forfettario");

                        long transferType = r.Field<byte>("dbf_Tipo_Trasf");
                        f.TransferType = transferType != 4 ? transferType : 0;                        

                        db.Factories.AddOrUpdate(f);
                        db.SaveChanges();

                        _factories.Add(r.Field<int>("dbf_Index"), f.Id);
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool CompileHourTable()
        {
            bool result = false;

            using (DBArchive db = new DBArchive())
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
                                    t.FDL = FormatFDL(r.Field<string>("Dbf_Foglio"));
                                    db.Timesheets.Add(t);
                                }

                                //Add details for second FDL
                                if (!(string.IsNullOrEmpty(r.Field<string>("Dbf_SecondoFoglio")) | string.IsNullOrWhiteSpace(r.Field<string>("Dbf_SecondoFoglio"))))
                                {
                                    t.FDL = FormatFDL(r.Field<string>("Dbf_Foglio"));
                                    db.Timesheets.Add(t);
                                }

                                // Factory association
                                long factoryId = r.Field<short>("Dbf_Impianto");
                                if (_factories.ContainsKey(factoryId))
                                {
                                    FDL fdl = db.FDLs.SingleOrDefault(f => f.Id == t.FDL);

                                    if (!fdl.Factory.HasValue)
                                    {
                                        fdl.Factory = _factories[factoryId];
                                        db.FDLs.AddOrUpdate(fdl);
                                    }
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

        private string FormatFDL(string fdl_Id)
        {   
            string[] parts = fdl_Id.Split('/');

            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            return $"{parts[1]}/{parts[0]}";
        }

        private bool CompileFdlTable()
        {
            bool result = false;
            
            try
            {
                IEnumerable<DataRow> sentFiles = dtSentFiles.Rows.Cast<DataRow>();

                using (DBArchive db = new DBArchive())
                {
                    foreach (string s in GetFileList(_sourceFdlPath))
                    {
                        try
                        {
                            FDLManager manager = SimpleIoc.Default.GetInstance<FDLManager>();
                            FDL fdl = manager.ImportFDLFromFile(s, false, false, true, true, true);

                            if (fdl != null)
                            {
                                File.Copy(s, Path.Combine(ApplicationSettings.Directories.FDL, new FileInfo(s).Name), true);

                                DataRow sent = sentFiles.Where(file => !string.IsNullOrEmpty(file.Field<string>("Dbf_Foglio")) && FormatFDL(file.Field<string>("Dbf_Foglio")) == fdl.Id && file.Field<int>("dbf_TipoInvio") == 2).Select(file => file).FirstOrDefault();

                                if (sent != null)
                                {
                                    // we must override recived fdl with the same of current dbcontext istance
                                    fdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);

                                    if (sent.Field<int>("Dbf_NumeroInviiPrima") == 0)
                                        fdl.EStatus = EFDLStatus.Waiting;
                                    else if (sent.Field<string>("Dbf_Impianto") != string.Empty && sent.Field<string>("Dbf_Commessa") != string.Empty)
                                        fdl.EStatus = EFDLStatus.Accepted;
                                    else
                                        fdl.EStatus = EFDLStatus.Cancelled;

                                    db.FDLs.AddOrUpdate(fdl);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Debugger.Break();
                        }
                    }

                    db.SaveChanges();
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
            using (DBArchive db = new DBArchive())
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
