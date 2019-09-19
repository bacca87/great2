using GalaSoft.MvvmLight.Ioc;
using Great.Models;
using Great.Models.Database;
using Great.Utils.Extensions;
using Great.ViewModels.Database;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading;

namespace Great.Utils
{
    public class GreatImport : BaseImport
    {
        #region Constants

        public const string sGreatDefaultInstallationFolder = @"C:\Program Files (x86)\GREAT";
        const string sAccessConnectionstring = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";
        const string sSqliteConnectionString = @"data source={0}";
        const string sGreatIniFilePath = @"Ini\Setting.ini";
        string[] sPathToCheck = new string[] { @"C:\Program Files" };

        #endregion

        #region Properties
        private FDLManager FDLManager = SimpleIoc.Default.GetInstance<FDLManager>();
        private volatile bool stopImport;

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
        public string _sourceEAPath { get; private set; }
        public string _destinationFdlPath { get; private set; }
        public string _destinationEAPath { get; private set; }
        public string GreatPath { get; set; }
        #endregion

        public GreatImport() : base(LogManager.GetLogger("GreatImport")) { }

        public override void Start()
        {
            stopImport = false;

            StatusChanged("Import Started...");

            if (Directory.Exists(GreatPath))
            {
                _sourceDatabase = GetGreatDatabaseFile(GreatPath);

                if (!string.IsNullOrEmpty(_sourceDatabase))
                {
                    if (GetDataTables())
                    {
                        bool start = true;

                        _sourceFdlPath = dtConfiguration.AsEnumerable().Where(x => x.Field<byte>("Dbf_File") == 5).Select(x => x.Field<string>("Dbf_Path")).FirstOrDefault();
                        _sourceEAPath = dtConfiguration.AsEnumerable().Where(x => x.Field<byte>("Dbf_File") == 8).Select(x => x.Field<string>("Dbf_Path")).FirstOrDefault();

                        if (!Directory.Exists(_sourceFdlPath))
                        {
                            start = false;
                            Error($"FDLs folder not found: {_sourceFdlPath}");
                        }

                        if (!Directory.Exists(_sourceEAPath))
                        {
                            start = false;
                            Error($"Expense accounts folder not found: {_sourceEAPath}");
                        }

                        if (start)
                        {
                            thrd = new Thread(new ThreadStart(ImportThread));
                            thrd.Start();
                            return;
                        }
                    }
                }
                else Error($"Database not found on path: {_sourceDatabase}");
            }
            else Error($"Wrong GREAT directory path: {GreatPath}");

            StatusChanged("Import failed!");
            Finished(false);
        }

        public override void Cancel()
        {
            stopImport = true;
        }

        public override void Close()
        {
            connection.Dispose();
        }

        private void ImportThread()
        {
            CompileFactoriesTable();
            CompileFdlTable();
            CompileHourTable();
            CompileEATable();
            CompileCarRents();

            if (!stopImport)
            {
                StatusChanged("Operation Completed!");
                Finished();
            }
            else
            {
                Message("Import stopped by user.");
                Finished(false);
            }
        }

        private void ClearCache()
        {
            dtConfiguration.Clear();
            dtHours.Clear();
            dtExpenseReview.Clear();
            dtCars.Clear();
            dtPlants.Clear();
            dtSentFiles.Clear();
            _factories.Clear();
        }

        private bool GetDataTables()
        {
            bool result = false;

            try
            {
                StatusChanged("Loading GREAT data...");

                ClearCache();

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
            bool result = false;
            IDictionary<string, long> _cars = new Dictionary<string, long>();

            if (stopImport)
                return result;

            StatusChanged("Importing Car Rents...");

            try
            {
                using (DBArchive db = new DBArchive())
                {

                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtCars.Rows.Cast<DataRow>();

                    var cars = collection.GroupBy(c => c.Field<string>("dbf_Targa")).Select(c => c.First());

                    // Import Cars
                    foreach (DataRow r in cars)
                    {
                        if (stopImport)
                            break;

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
                        catch (Exception ex)
                        {
                            Error($"Failed to import the car {car.LicensePlate}. {ex}", ex);
                        }
                    }

                    // Import Rents
                    foreach (DataRow r in collection)
                    {
                        if (stopImport)
                            break;

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
                            catch (Exception ex)
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

            if (stopImport)
                return result;

            StatusChanged("Importing Factories...");

            try
            {
                using (DBArchive db = new DBArchive())
                {
                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtPlants.Rows.Cast<DataRow>();

                    foreach (DataRow r in collection)
                    {
                        if (stopImport)
                            break;

                        Factory f = new Factory();

                        try
                        {
                            f.Name = r.Field<string>("dbf_Stabilimento");
                            f.CompanyName = r.Field<string>("dbf_RagioneSociale");
                            f.Address = r.Field<string>("dbf_Indirizzo");
                            f.IsForfait = r.Field<bool>("dbf_Forfettario");

                            long transferType = r.Field<byte>("dbf_Tipo_Trasf");
                            f.TransferType = transferType != 4 ? transferType : 0;

                            db.Factories.AddOrUpdate(x => x.Name, f);
                            db.SaveChanges();

                            _factories.Add(r.Field<int>("dbf_Index"), f.Id);

                            Message($"Factory {f.Name} OK");
                        }
                        catch (Exception ex)
                        {
                            Error($"Failed to import factory {f.Name}. {ex}", ex);
                        }
                    }

                    db.SaveChanges();

                    result = true;
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

            if (stopImport)
                return result;

            StatusChanged("Importing Hours...");

            try
            {
                using (DBArchive db = new DBArchive())
                {
                    //Get enumerable rows fron datatable
                    IEnumerable<DataRow> collection = dtHours.Rows.Cast<DataRow>();

                    foreach (DataRow r in collection)
                    {
                        if (stopImport)
                            break;

                        DayEVM d = new DayEVM();

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

                            d.Save(db);

                            //Add office hours
                            if (r.Field<Int16>("Dbf_Uff_Inizio_AM") != 0 |
                                r.Field<Int16>("Dbf_Uff_Fine_AM") != 0 |
                                r.Field<Int16>("Dbf_Uff_Inizio_PM") != 0 |
                                r.Field<Int16>("Dbf_Uff_Fine_PM") != 0)
                            {
                                // clean all office timesheets
                                //db.Timesheets.RemoveRange(db.Timesheets.Where(t => t.Timestamp == d.Timestamp && (t.FDL == null || t.FDL == string.Empty)));

                                Timesheet office = new Timesheet();
                                office.Timestamp = d.Timestamp;

                                office.TravelStartTimeAM = null;
                                office.WorkStartTimeAM = r.Field<Int16>("Dbf_Uff_Inizio_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Inizio_AM")).TotalSeconds : null;
                                office.WorkEndTimeAM = r.Field<Int16>("Dbf_Uff_Fine_AM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Fine_AM")).TotalSeconds : null;
                                office.TravelEndTimeAM = null;
                                office.TravelStartTimePM = null;
                                office.WorkStartTimePM = r.Field<Int16>("Dbf_Uff_Inizio_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Inizio_PM")).TotalSeconds : null;
                                office.WorkEndTimePM = r.Field<Int16>("Dbf_Uff_Fine_PM") > 0 ? (long?)TimeSpan.FromMinutes(r.Field<Int16>("Dbf_Uff_Fine_PM")).TotalSeconds : null;
                                office.TravelEndTimePM = null;

                                db.Timesheets.AddOrUpdate(x => new { x.Timestamp, x.FDL }, office);
                            }

                            // Factory association
                            bool IsRestDay = r.Field<Int16>("Dbf_Uff_Inizio_AM") == 0 &&
                                        r.Field<Int16>("Dbf_Uff_Fine_AM") == 0 &&
                                        r.Field<Int16>("Dbf_Uff_Inizio_PM") == 0 &&
                                        r.Field<Int16>("Dbf_Uff_Fine_PM") == 0 &&
                                        r.Field<Int16>("Dbf_Trasf_Inizio_AM") == 0 &&
                                        r.Field<Int16>("Dbf_Trasf_Fine_AM") == 0 &&
                                        r.Field<Int16>("Dbf_Trasf_Inizio_PM") == 0 &&
                                        r.Field<Int16>("Dbf_Trasf_Fine_PM") == 0 &&
                                        r.Field<Int16>("Dbf_Partenza_AM") == 0 &&
                                        r.Field<Int16>("Dbf_Arrivo_AM") == 0 &&
                                        r.Field<Int16>("Dbf_Partenza_PM") == 0 &&
                                        r.Field<Int16>("Dbf_Arrivo_PM") == 0;

                            short? factoryId = r.Field<short?>("Dbf_Impianto");
                            string fdlId = FormatFDL(r.Field<string>("Dbf_Foglio"));

                            if (factoryId.HasValue && _factories.ContainsKey(factoryId.Value) && !string.IsNullOrEmpty(fdlId))
                            {
                                FDL fdl = db.FDLs.SingleOrDefault(f => f.Id == fdlId);

                                if (fdl != null)
                                {
                                    if (!fdl.Factory.HasValue)
                                    {
                                        fdl.Factory = _factories[factoryId.Value];
                                        db.FDLs.AddOrUpdate(fdl);
                                        db.SaveChanges();
                                    }

                                    // assign fdl to rest day if present
                                    if (IsRestDay && !db.Timesheets.Any(x => x.Timestamp == d.Timestamp && fdlId == x.FDL))
                                    {
                                        TimesheetEVM restDay = new TimesheetEVM();
                                        restDay.Timestamp = d.Timestamp;
                                        restDay.FDL = fdlId;
                                        restDay.Save(db);
                                    }
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
                                    db.SaveChanges();
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

        private bool CompileFdlTable()
        {
            bool result = false;

            if (stopImport)
                return result;

            StatusChanged("Importing PDF files...");

            try
            {
                IEnumerable<DataRow> sentFiles = dtSentFiles.Rows.Cast<DataRow>();

                foreach (FileInfo file in new DirectoryInfo(_sourceFdlPath).GetFiles("*.pdf", SearchOption.AllDirectories))
                {
                    if (stopImport)
                        break;

                    FDLEVM fdl = null;

                    try
                    {
                        fdl = FDLManager.ImportFDLFromFile(file.FullName, false, false, false, true, true);

                        // try with XFA format
                        if (fdl == null)
                            fdl = FDLManager.ImportFDLFromFile(file.FullName, true, false, false, true, true);

                        if (fdl != null)
                        {
                            File.Copy(file.FullName, Path.Combine(ApplicationSettings.Directories.FDL, file.Name), true);

                            DataRow sent = sentFiles.Where(f => !string.IsNullOrEmpty(f.Field<string>("Dbf_Foglio")) && FormatFDL(f.Field<string>("Dbf_Foglio")) == fdl.Id && (f.Field<int>("dbf_TipoInvio") == 2 || f.Field<int>("dbf_TipoInvio") == 4)).Select(f => f)
                                                    .OrderBy(x => x.Field<int>("Dbf_NumeroInviiPrima") == 0)
                                                    .ThenBy(x => string.IsNullOrEmpty(x.Field<string>("Dbf_Impianto")))
                                                    .ThenBy(x => string.IsNullOrEmpty(x.Field<string>("Dbf_Commessa")))
                                                    .FirstOrDefault();

                            if (sent != null)
                            {
                                using (DBArchive db = new DBArchive())
                                {
                                    // we must override recived fdl with the same of current dbcontext istance
                                    FDL currentFdl = db.FDLs.SingleOrDefault(f => f.Id == fdl.Id);

                                    if (currentFdl != null)
                                    {
                                        if (sent.Field<int>("Dbf_NumeroInviiPrima") == 0)
                                            currentFdl.Status = (long)EFDLStatus.Waiting;
                                        else if (sent.Field<string>("Dbf_Impianto") != string.Empty && sent.Field<string>("Dbf_Commessa") != string.Empty)
                                            currentFdl.Status = (long)EFDLStatus.Accepted;
                                        else
                                            currentFdl.Status = (long)EFDLStatus.Cancelled;

                                        if (currentFdl.Status != (long)EFDLStatus.New)
                                        {
                                            currentFdl.IsReadOnly = true;
                                            currentFdl.IsCompiled = true;
                                        }   

                                        db.FDLs.AddOrUpdate(currentFdl);
                                        db.SaveChanges();
                                        Message($"FDL {currentFdl.Id} OK");
                                    }
                                    else
                                        Error("Missing FDL on database. Should never happen.");
                                }
                            }
                            else
                                Error("Missing FDL sent status!");
                        }
                        else
                            Error($"Failed to import FDL from file: {file.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Error($"Failed importing FDL {fdl?.Id}. {ex}", ex);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                Error($"Failed importing FDLs. {ex}", ex);
            }

            return result;
        }

        private bool CompileEATable()
        {
            bool result = false;

            if (stopImport)
                return result;

            StatusChanged("Importing Expense Account files...");

            try
            {
                IEnumerable<DataRow> expenses = dtExpenseReview.Rows.Cast<DataRow>();

                foreach (FileInfo file in new DirectoryInfo(_sourceEAPath).GetFiles("*.pdf", SearchOption.AllDirectories))
                {
                    if (stopImport)
                        break;

                    ExpenseAccountEVM ea = null;

                    try
                    {
                        ea = FDLManager.ImportEAFromFile(file.FullName, false, false, true);

                        if (ea != null)
                        {
                            File.Copy(file.FullName, Path.Combine(ApplicationSettings.Directories.ExpenseAccount, file.Name), true);

                            using (DBArchive db = new DBArchive())
                            {
                                // we must override recived EA with the same of current dbcontext istance
                                ExpenseAccount currentEA = db.ExpenseAccounts.SingleOrDefault(e => e.Id == ea.Id);

                                if (currentEA != null)
                                {
                                    FDL fdl = db.FDLs.SingleOrDefault(f => f.Id == currentEA.FDL);

                                    if (fdl != null)
                                        currentEA.Status = fdl.Status;
                                    else
                                        currentEA.Status = (long)EFDLStatus.Accepted;

                                    var expense = expenses.SingleOrDefault(e => !string.IsNullOrEmpty(e.Field<string>("Dbf_Foglio")) && FormatFDL(e.Field<string>("Dbf_Foglio")) == fdl.Id);
                                    currentEA.IsRefunded = expense != null && expense.Field<bool>("Dbf_Restituito");

                                    if (currentEA.Status != (long)EFDLStatus.New)
                                    {
                                        currentEA.IsReadOnly = true;
                                        currentEA.IsCompiled = true;
                                    }   

                                    db.ExpenseAccounts.AddOrUpdate(currentEA);
                                    db.SaveChanges();
                                    Message($"Expense Account {currentEA.FDL} OK");
                                }
                                else
                                    Error("Missing EA on database. Should never happen.");
                            }
                        }
                        else
                            Error($"Failed to import EA from file: {file.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Error($"Failed importing EA {ea?.FDL}. {ex}", ex);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                Error($"Failed importing expense accounts. {ex}", ex);
            }

            return result;
        }

        private string GetGreatDatabaseFile(string folder)
        {
            var uri = new DirectoryInfo(folder);
            string virtualStorePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualStore\\", uri.Parent.Name), Path.Combine(uri.Name, "DB\\Archivio.mdb"));

            if (File.Exists(virtualStorePath))
                return virtualStorePath;
            else
                return (Path.Combine(folder, "DB\\Archivio.mdb"));
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
    }
}
