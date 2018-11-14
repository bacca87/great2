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
using NLog;

namespace Great.Utils
{
    public class GreatImport
    {
        #region Events
        public delegate void OperationCompletedHandler(object source);
        public delegate void StatusChangedHandler(object source, GreatImportArgs args);
        public delegate void MessageHandler(object source, GreatImportArgs args);
        public event OperationCompletedHandler OnCompleted;
        public event StatusChangedHandler OnStatusChanged;
        public event MessageHandler OnMessage;
        #endregion

        #region Constants

        public const string sGreatDefaultInstallationFolder = @"C:\Program Files (x86)\GREAT";
        const string sAccessConnectionstring = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";
        const string sSqliteConnectionString = @"data source={0}";
        const string sGreatIniFilePath = @"Ini\Setting.ini";
        string[] sPathToCheck = new string[] { @"C:\Program Files" };

        #endregion

        #region Properties
        private readonly Logger log = LogManager.GetLogger("GreatImport");

        private FDLManager FDLManager = SimpleIoc.Default.GetInstance<FDLManager>();

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

        // Temp Cache
        private IDictionary<long, long> _factories = new Dictionary<long, long>();

        public string _sourceDatabase { get; private set; }
        public string _sourceFdlPath { get; private set; }
        public string _sourceAccountPath { get; private set; }
        public string _destinationFdlPath { get; private set; }
        public string _destinationAccountPath { get; private set; }
        #endregion

        public void StartMigration(string greatPath)
        {
            StatusChanged("Migration Started...");

            if (Directory.Exists(greatPath))
            {
                _sourceDatabase = GetGreatDatabaseFile(greatPath);

                if (!string.IsNullOrEmpty(_sourceDatabase))
                {
                    if (GetDataTables())
                    {
                        bool start = true;

                        _sourceFdlPath = dtConfiguration.AsEnumerable().Where(x => x.Field<byte>("Dbf_File") == 5).Select(x => x.Field<string>("Dbf_Path")).FirstOrDefault();
                        _sourceAccountPath = dtConfiguration.AsEnumerable().Where(x => x.Field<byte>("Dbf_File") == 8).Select(x => x.Field<string>("Dbf_Path")).FirstOrDefault();

                        if (!Directory.Exists(_sourceFdlPath))
                        {
                            start = false;
                            Error($"FDLs folder not found: {_sourceFdlPath}");
                        }

                        if (!Directory.Exists(_sourceAccountPath))
                        {
                            start = false;
                            Error($"Expense accounts folder not found: {_sourceAccountPath}");
                        }

                        if (start)
                        {
                            thrd = new Thread(new ThreadStart(MigrationThread));
                            thrd.Start();
                            return;
                        }
                    }
                }
                else Error($"Database not found on path: {_sourceDatabase}");
            }
            else Error($"Wrong GREAT directory path: {greatPath}");

            StatusChanged("Import failed!");
        }

        private void MigrationThread()
        {
            StatusChanged("Importing Factories...");
            CompileFactoriesTable();
            StatusChanged("Importing PDF files...");
            CompileFdlTable();
            StatusChanged("Importing Hours...");
            CompileHourTable();
            StatusChanged("Importing Car Rents...");
            CompileCarRents();
            StatusChanged("Operation Completed!");
            Completed();
        }

        private bool GetDataTables()
        {
            bool result = false;

            try
            {
                StatusChanged("Loading GREAT data...");

                Message("Open GREAT database");
                connection = new OleDbConnection(string.Format(sAccessConnectionstring, _sourceDatabase));
                connection.Open();

                command = new OleDbCommand("SELECT * FROM dbt_File_Config", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtConfiguration);
                Message("Configurations loaded");

                command = new OleDbCommand("SELECT * FROM dbt_Ore order by Dbf_Data", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtHours);
                Message("Timesheets loaded");

                command = new OleDbCommand("SELECT * FROM dbt_NotaSpese order by Dbf_dtConsegna", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtExpenseReview);
                Message("Expense account loaded");

                command = new OleDbCommand("SELECT * FROM dbt_Auto order by Dbf_DataPrel", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtCars);
                Message("Car rental loaded");

                command = new OleDbCommand("SELECT * FROM dbt_Stabilimenti", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtPlants);
                Message("Factories loaded");

                command = new OleDbCommand("SELECT * FROM dbt_Invii", connection);
                adapter = new OleDbDataAdapter(command);
                adapter.Fill(dtSentFiles);
                Message("Sent items loaded");

                Close();

                result = true;

                StatusChanged("GREAT data loaded!");
            }
            catch (Exception ex)
            {
                result = false;
                Error($"Error during load GREAT data: {ex}", ex);
            }

            return result;
        }

        private bool CompileCarRents()
        {
            IDictionary<string, long> _cars = new Dictionary<string, long>();
            bool result = false;

            try
            {
                using (DBArchive db = new DBArchive())
                {
                    Timesheet t = null;

                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtCars.Rows.Cast<DataRow>();

                    var cars = collection.GroupBy(c => c.Field<string>("dbf_Targa")).Select(c => c.First());

                    // Import Cars
                    foreach (DataRow r in cars)
                    {
                        Car car = new Car();

                        try
                        {
                            car.LicensePlate = r.Field<string>("dbf_Targa").Trim();
                            car.Brand = r.Field<string>("dbf_Marca").Trim();
                            car.Model = r.Field<string>("dbf_Modello").Trim();
                            car.CarRentalCompany = r.Field<short>("dbf_Nolo");

                            db.Cars.AddOrUpdate(x => x.LicensePlate, car);
                            db.SaveChanges();

                            _cars.Add(r.Field<string>("dbf_Targa"), car.Id);

                            Message($"Car {car.LicensePlate} | {car.Brand} {car.Model} OK");
                        }
                        catch(Exception ex)
                        {
                            Error($"Failed to import the car {car.LicensePlate}. {ex}", ex);
                        }
                    }

                    // Import Rents
                    foreach (DataRow r in collection)
                    {
                        string licensePlate = r.Field<string>("dbf_Targa");
                        if (_cars.ContainsKey(licensePlate))
                        {
                            CarRentalHistory his = new CarRentalHistory();
                            try
                            {
                                his.Car = _cars[licensePlate];
                                his.StartKm = r.Field<int>("dbf_KmInizio");
                                his.EndKm = r.Field<int>("dbf_KmFine");
                                his.StartLocation = r.Field<string>("dbf_LuogoPrel").Trim();
                                his.EndLocation = r.Field<string>("dbf_LuogoDepo").Trim();

                                his.StartDate = (r.Field<DateTime>("dbf_DataPrel") + r.Field<DateTime>("dbf_OraPrel").TimeOfDay).ToUnixTimestamp();
                                his.EndDate = (r.Field<DateTime>("dbf_DataDepo") + r.Field<DateTime>("dbf_OraDepo").TimeOfDay).ToUnixTimestamp();

                                his.StartFuelLevel = r.Field<short>("dbf_SerbPrel");
                                his.EndFuelLevel = r.Field<short>("dbf_SerbRicon");
                                his.Notes = r.Field<string>("dbf_Note").Trim();

                                db.CarRentalHistories.AddOrUpdate(x => x.StartDate, his);

                                Message($"Rent {DateTime.Now.FromUnixTimestamp(his.StartDate).ToShortDateString()} Car {licensePlate} OK");
                            }
                            catch(Exception ex)
                            {
                                Error($"Failed to import the rent in date {DateTime.Now.FromUnixTimestamp(his.StartDate).ToShortDateString()}, Car {licensePlate}. {ex}", ex);
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Error($"Failed importing car rents. {ex}", ex);
            }

            return result;
        }

        private bool CompileFactoriesTable()
        {
            bool result = false;
            try
            {
                using (DBArchive db = new DBArchive())
                {
                    Timesheet t = null;

                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtPlants.Rows.Cast<DataRow>();

                    foreach (DataRow r in collection)
                    {
                        Factory f = new Factory();

                        try
                        {   
                            f.Name = r.Field<string>("dbf_Stabilimento");
                            f.CompanyName = r.Field<string>("dbf_RagioneSociale");
                            f.Address = r.Field<string>("dbf_Indirizzo");
                            f.IsForfait = r.Field<bool>("dbf_Forfettario");

                            long transferType = r.Field<byte>("dbf_Tipo_Trasf");
                            f.TransferType = transferType != 4 ? transferType : 0;

                            db.Factories.AddOrUpdate(f);
                            db.SaveChanges();

                            _factories.Add(r.Field<int>("dbf_Index"), f.Id);

                            Message($"Factory {f.Name} OK");
                        }
                        catch (Exception ex)
                        {
                            Error($"Failed to import factory {f.Name}. {ex}", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                Error($"Failed importing factories. {ex}", ex);
            }

            return result;
        }

        private bool CompileHourTable()
        {
            bool result = false;

            try
            {
                using (DBArchive db = new DBArchive())
                {
                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtHours.Rows.Cast<DataRow>();

                    foreach (DataRow r in collection)
                    {
                        Day d = new Day();

                        try
                        {
                            d.Timestamp = r.Field<DateTime>("Dbf_Data").ToUnixTimestamp();

                            d.Type = r.Field<byte>("dbf_Tipo_Giorno");

                            if (d.Type != 3 && d.Type != 6)
                                d.Type = 0;
                            else if (d.Type == 3)
                                d.Type = 1;
                            else if (d.Type == 6)
                                d.Type = 2;

                            db.Days.AddOrUpdate(d);

                            //t = new Timesheet();
                            long timestamp = r.Field<DateTime>("Dbf_Data").ToUnixTimestamp();

                            //Add office hours
                            if (
                                r.Field<Int16>("Dbf_Uff_Inizio_AM") != 0 |
                                r.Field<Int16>("Dbf_Uff_Fine_AM") != 0 |
                                r.Field<Int16>("Dbf_Uff_Inizio_PM") != 0 |
                                r.Field<Int16>("Dbf_Uff_Fine_PM") != 0)
                            {
                                Timesheet office = new Timesheet();
                                office.Timestamp = timestamp;

                                office.TravelStartTimeAM = null;
                                office.WorkStartTimeAM = r.Field<Int16>("Dbf_Uff_Inizio_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Inizio_AM")).TotalSeconds : null;
                                office.WorkEndTimeAM = r.Field<Int16>("Dbf_Uff_Fine_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Fine_AM")).TotalSeconds : null;
                                office.TravelEndTimeAM = null;
                                office.TravelStartTimePM = null;
                                office.WorkStartTimePM = r.Field<Int16>("Dbf_Uff_Inizio_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Inizio_PM")).TotalSeconds : null;
                                office.WorkEndTimePM = r.Field<Int16>("Dbf_Uff_Fine_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Fine_PM")).TotalSeconds : null;
                                office.TravelEndTimePM = null;

                                if(db.Timesheets.Where(x => x.Timestamp == office.Timestamp && office.FDL == string.Empty).Count() == 0)
                                    db.Timesheets.Add(office);
                            }

                            // Factory association
                            short? factoryId = r.Field<short?>("Dbf_Impianto");
                            string fdlId = FormatFDL(r.Field<string>("Dbf_Foglio"));

                            if (factoryId.HasValue && _factories.ContainsKey(factoryId.Value) && !string.IsNullOrEmpty(fdlId))
                            {
                                FDL fdl = db.FDLs.SingleOrDefault(f => f.Id == fdlId);

                                if (fdl != null && !fdl.Factory.HasValue)
                                {
                                    fdl.Factory = _factories[factoryId.Value];
                                    db.FDLs.AddOrUpdate(fdl);
                                }
                                else
                                    Warning($"The FDL {fdlId} is missing on database. Impossible to assign the factory to the current timesheet. Day: {d.Date.ToShortDateString()}");
                            }

                            short? factory2Id = r.Field<short?>("Dbf_SecondoImpianto");
                            string fdl2Id = FormatFDL(r.Field<string>("Dbf_SecondoFoglio"));

                            if (factory2Id.HasValue && _factories.ContainsKey(factory2Id.Value) && !string.IsNullOrEmpty(fdl2Id))
                            {
                                FDL fdl = db.FDLs.SingleOrDefault(f => f.Id == fdl2Id);

                                if (fdl != null && !fdl.Factory.HasValue)
                                {
                                    fdl.Factory = _factories[factory2Id.Value];
                                    db.FDLs.AddOrUpdate(fdl);
                                }
                                else
                                    Warning($"The second FDL {fdlId} is missing on database. Impossible to assign the factory to the current timesheet. Day: {d.Date.ToShortDateString()}");
                            }

                            Message($"Day {d.Date.ToShortDateString()} OK");
                        }
                        catch (Exception ex)
                        {
                            Error($"Failed to import the Timesheet {d.Date.ToShortDateString()}. {ex}", ex);
                        }
                    }

                    db.SaveChanges();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Error($"Failed importing the timesheets. {ex}", ex);
            }

            return result;
        }

        private string FormatFDL(string fdl_Id)
        {
            if (string.IsNullOrEmpty(fdl_Id))
                return string.Empty;

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
                    foreach (string fdlPath in GetFileList(_sourceFdlPath))
                    {
                        FDL fdl = null;

                        try
                        {
                            fdl = FDLManager.ImportFDLFromFile(fdlPath, false, false, false, true, true);

                            // try with XFA format
                            if(fdl == null)
                                fdl = FDLManager.ImportFDLFromFile(fdlPath, true, false, false, true, true);

                            if (fdl != null)
                            {
                                File.Copy(fdlPath, Path.Combine(ApplicationSettings.Directories.FDL, new FileInfo(fdlPath).Name), true);

                                DataRow sent = sentFiles.Where(file => !string.IsNullOrEmpty(file.Field<string>("Dbf_Foglio")) && FormatFDL(file.Field<string>("Dbf_Foglio")) == fdl.Id && file.Field<int>("dbf_TipoInvio") == 2).Select(file => file).FirstOrDefault();

                                if (sent != null)
                                {
                                    // we must override recived fdl with the same of current dbcontext istance
                                    FDL currentFdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);

                                    if (currentFdl != null)
                                    {
                                        if (sent.Field<int>("Dbf_NumeroInviiPrima") == 0)
                                            currentFdl.EStatus = EFDLStatus.Waiting;
                                        else if (sent.Field<string>("Dbf_Impianto") != string.Empty && sent.Field<string>("Dbf_Commessa") != string.Empty)
                                            currentFdl.EStatus = EFDLStatus.Accepted;
                                        else
                                            currentFdl.EStatus = EFDLStatus.Cancelled;

                                        db.FDLs.AddOrUpdate(currentFdl);
                                        Message($"FDL {fdl.Id} OK");
                                    }
                                    else
                                        Error("MERDAAAAAAAAAAAAAAAAAAAAAAA");
                                }
                                else
                                    Error("Missing sent status!");
                            }
                            else
                                Error($"Failed to import FDL from file: {fdlPath}");
                        }
                        catch (Exception ex)
                        {
                            Error($"Failed importing FDL {fdl?.Id}. {ex}", ex);
                        }
                    }

                    db.SaveChanges();
                }

                result = true;
            }
            catch (Exception ex)
            {
                Error($"Failed importing FDLs. {ex}", ex);
            }

            return result;
        }

        #region Auxiliar methods
        private string GetGreatDatabaseFile(string folder)
        {
            var uri = new DirectoryInfo(folder);
            string virtualStorePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\", uri.Parent.Name), Path.Combine(uri.Name, "DB\\Archivio.mdb"));
            
            if (File.Exists(virtualStorePath))
                return virtualStorePath;
            else
                return (Path.Combine(folder, "DB\\Archivio.mdb"));
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
        protected void StatusChanged(string status)
        {
            OnStatusChanged?.Invoke(this, new GreatImportArgs(status));
            OnMessage?.Invoke(this, new GreatImportArgs(status));
            log.Info(status);
        }

        protected void Warning(string message)
        {
            OnMessage?.Invoke(this, new GreatImportArgs($"WARNING: {message}"));
            log.Warn(message);
        }

        protected void Error(string message, Exception ex = null)
        {
            OnMessage?.Invoke(this, new GreatImportArgs($"ERROR: {message}"));
            log.Error(ex, message);
        }

        protected void Message(string message)
        {
            OnMessage?.Invoke(this, new GreatImportArgs(message));
            log.Debug(message);
        }

        protected void Completed()
        {
            OnCompleted?.Invoke(this);
        }
        #endregion
    }

    public class GreatImportArgs : EventArgs
    {
        public GreatImportArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
