using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Models.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great.ViewModels.Database
{
    public class FDLEVM : EntityViewModelBase, IFDLFile
    {
        #region Properties
        private string _Id;
        public string Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private long _WeekNr;
        public long WeekNr
        {
            get => _WeekNr;
            set => Set(ref _WeekNr, value);
        }

        private long _StartDay;
        public long StartDay
        {
            get => _StartDay;
            set => Set(ref _StartDay, value);
        }

        private bool _IsExtra;
        public bool IsExtra
        {
            get => _IsExtra;
            set => Set(ref _IsExtra, value);
        }

        private long _Order;
        public long Order
        {
            get => _Order;
            set => Set(ref _Order, value);
        }

        private long? _Factory;
        public long? Factory
        {
            get => _Factory;
            set => Set(ref _Factory, value);
        }

        private bool _OutwardCar;
        public bool OutwardCar
        {
            get => _OutwardCar;
            set => Set(ref _OutwardCar, value);
        }

        private bool _ReturnCar;
        public bool ReturnCar
        {
            get => _ReturnCar;
            set => Set(ref _ReturnCar, value);
        }

        private bool _OutwardTaxi;
        public bool OutwardTaxi
        {
            get => _OutwardTaxi;
            set => Set(ref _OutwardTaxi, value);
        }

        private bool _ReturnTaxi;
        public bool ReturnTaxi
        {
            get => _ReturnTaxi;
            set => Set(ref _ReturnTaxi, value);
        }

        private bool _OutwardAircraft;
        public bool OutwardAircraft
        {
            get => _OutwardAircraft;
            set => Set(ref _OutwardAircraft, value);
        }

        private bool _ReturnAircraft;
        public bool ReturnAircraft
        {
            get => _ReturnAircraft;
            set => Set(ref _ReturnAircraft, value);
        }

        private string _PerformanceDescription;
        public string PerformanceDescription
        {
            get => _PerformanceDescription;
            set => Set(ref _PerformanceDescription, value);
        }

        private long _Result;
        public long Result
        {
            get => _Result;
            set
            {
                Set(ref _Result, value);
                RaisePropertyChanged(nameof(EResult));
            }
        }

        private string _ResultNotes;
        public string ResultNotes
        {
            get => _ResultNotes;
            set => Set(ref _ResultNotes, value);
        }

        private string _Notes;
        public string Notes
        {
            get => _Notes;
            set => Set(ref _Notes, value);
        }

        private string _PerformanceDescriptionDetails;
        public string PerformanceDescriptionDetails
        {
            get => _PerformanceDescriptionDetails;
            set => Set(ref _PerformanceDescriptionDetails, value);
        }

        private long _Status;
        public long Status
        {
            get => _Status;
            set
            {
                Set(ref _Status, value);
                RaisePropertyChanged(nameof(EStatus));
            }
        }

        private string _LastError;
        public string LastError
        {
            get => _LastError;
            set => Set(ref _LastError, value);
        }

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set => Set(ref _FileName, value);
        }

        private bool _NotifyAsNew;
        public bool NotifyAsNew
        {
            get => _NotifyAsNew;
            set => Set(ref _NotifyAsNew, value);
        }

        private bool _IsCompiled;
        public bool IsCompiled
        {
            get => _IsCompiled;
            set => Set(ref _IsCompiled, value);
        }

        private bool _IsReadOnly;
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set => Set(ref _IsReadOnly, value);
        }

        private FactoryDTO _Factory1;
        public FactoryDTO Factory1
        {
            get => _Factory1;
            set
            {
                Set(ref _Factory1, value);
                RaisePropertyChanged(nameof(FDL_Factory_Display));
            }
        }

        private FDLStatusDTO _FDLStatus;
        public FDLStatusDTO FDLStatus
        {
            get => _FDLStatus;
            set => Set(ref _FDLStatus, value);
        }

        private FDLResultDTO _FDLResult;
        public FDLResultDTO FDLResult
        {
            get => _FDLResult;
            set => Set(ref _FDLResult, value);
        }

        public ObservableCollection<TimesheetEVM> Timesheets { get; set; }

        public string FilePath { get { return ApplicationSettings.Directories.FDL + FileName; } }

        public int Year { get { return Convert.ToInt32(Id.Substring(0, 4)); } }

        private bool _IsNew;
        public bool IsNew // used for sorting purpose
        {
            get => _IsNew;
            set => Set(ref _IsNew, value);
        } 

        public bool IsValid { get { return Timesheets.Count == 0 || Timesheets.All(ts => ts.IsValid); } }

        public EFDLStatus EStatus
        {
            get => (EFDLStatus)Status;
            set
            {
                Status = (long)value;
                IsNew = value == EFDLStatus.New;
                RaisePropertyChanged();
            }
        }

        public EFDLResult EResult
        {
            get => (EFDLResult)Result;
            set
            {
                Result = (long)value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Display Properties
        public string FDL_Display => $"{Id}{(IsExtra ? " (EXTRA)" : "")}";

        public string FDL_Factory_Display => $"{Id}{(Factory1 != null ? $" [{Factory1.Name}]" : "")}{(IsExtra ? " (EXTRA)" : "")}";
        #endregion
        
        // hack because XAML didnt support default parameters
        public FDLEVM() => Timesheets = new ObservableCollection<TimesheetEVM>();

        public FDLEVM(FDL fdl = null)
        {
            Timesheets = new ObservableCollection<TimesheetEVM>();
            
            if(fdl != null)
                Global.Mapper.Map(fdl, this);
        }

        public override bool Save(DBArchive db)
        {
            if (IsReadOnly)
                return false;

            FDL fdl = new FDL();

            //IsCompiled = false;

            Global.Mapper.Map(this, fdl);
            db.FDLs.AddOrUpdate(fdl);
            db.SaveChanges();

            return true;
        }

        public override bool Delete(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Refresh(DBArchive db)
        {
            FDL fdl = db.FDLs.SingleOrDefault(f=> f.Id == Id);

            if (fdl != null)
            {
                Global.Mapper.Map(fdl, this);

                //foreach (TimesheetEVM timesheet in Timesheets)
                //    timesheet.Refresh(db);

                return true;
            }

            return false;
        }
    }
}
