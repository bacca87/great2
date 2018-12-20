using AutoMapper;
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
        public string Id { get; set; }
        public long WeekNr { get; set; }
        public long StartDay { get; set; }
        public bool IsExtra { get; set; }
        public long Order { get; set; }

        public long? _Factory;
        public long? Factory
        {
            get => _Factory;
            set => Set(ref _Factory, value);
        }

        public bool _OutwardCar;
        public bool OutwardCar
        {
            get => _OutwardCar;
            set => Set(ref _OutwardCar, value);
        }

        public bool _ReturnCar;
        public bool ReturnCar
        {
            get => _ReturnCar;
            set => Set(ref _ReturnCar, value);
        }

        public bool _OutwardTaxi;
        public bool OutwardTaxi
        {
            get => _OutwardTaxi;
            set => Set(ref _OutwardTaxi, value);
        }

        public bool _ReturnTaxi;
        public bool ReturnTaxi
        {
            get => _ReturnTaxi;
            set => Set(ref _ReturnTaxi, value);
        }

        public bool _OutwardAircraft;
        public bool OutwardAircraft
        {
            get => _OutwardAircraft;
            set => Set(ref _OutwardAircraft, value);
        }

        public bool _ReturnAircraft;
        public bool ReturnAircraft
        {
            get => _ReturnAircraft;
            set => Set(ref _ReturnAircraft, value);
        }

        public string _PerformanceDescription;
        public string PerformanceDescription
        {
            get => _PerformanceDescription;
            set => Set(ref _PerformanceDescription, value);
        }

        public long _Result;
        public long Result
        {
            get => _Result;
            set => Set(ref _Result, value);
        }

        public string _ResultNotes;
        public string ResultNotes
        {
            get => _ResultNotes;
            set => Set(ref _ResultNotes, value);
        }

        public string _Notes;
        public string Notes
        {
            get => _Notes;
            set => Set(ref _Notes, value);
        }

        public string _PerformanceDescriptionDetails;
        public string PerformanceDescriptionDetails
        {
            get => _PerformanceDescriptionDetails;
            set => Set(ref _PerformanceDescriptionDetails, value);
        }

        public long _Status;
        public long Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        public string _LastError;
        public string LastError
        {
            get => _LastError;
            set => Set(ref _LastError, value);
        }

        public string _FileName;
        public string FileName
        {
            get => _FileName;
            set => Set(ref _FileName, value);
        }

        public bool _NotifyAsNew;
        public bool NotifyAsNew
        {
            get => _NotifyAsNew;
            set => Set(ref _NotifyAsNew, value);
        }

        public FactoryDTO _Factory1;
        public FactoryDTO Factory1
        {
            get => _Factory1;
            set => Set(ref _Factory1, value);
        }

        public FDLStatusDTO _FDLStatus;
        public FDLStatusDTO FDLStatus
        {
            get => _FDLStatus;
            set => Set(ref _FDLStatus, value);
        }

        public FDLResultDTO _FDLResult;
        public FDLResultDTO FDLResult
        {
            get => _FDLResult;
            set => Set(ref _FDLResult, value);
        }

        public ObservableCollection<TimesheetDTO> Timesheets { get; set; }

        public string FilePath { get { return ApplicationSettings.Directories.FDL + FileName; } }

        public int Year { get { return Convert.ToInt32(Id.Substring(0, 4)); } }

        public bool _IsNew;
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
                RaisePropertyChanged();
                Status = (long)value;
                IsNew = value == EFDLStatus.New;
            }
        }

        public EFDLResult EResult
        {
            get => (EFDLResult)Result;
            set
            {
                RaisePropertyChanged();
                Result = (long)value;
            }
        }
        #endregion

        #region Display Properties
        public string FDL_Display => $"{Id}{(IsExtra ? " (EXTRA)" : "")}";

        public string FDL_Factory_Display => $"{Id}{(Factory1 != null ? $" [{Factory1.Name}]" : "")}{(IsExtra ? " (EXTRA)" : "")}";
        #endregion

        public FDLEVM()
        {
            Timesheets = new ObservableCollection<TimesheetDTO>();
        }

        public FDLEVM(FDL fdl)
        {
            Timesheets = new ObservableCollection<TimesheetDTO>();
            
            Mapper.Map(fdl, this);
        }

        public override bool Save(DBArchive db)
        {
            FDL fdl = new FDL();

            Mapper.Map(this, fdl);
            db.FDLs.AddOrUpdate(fdl);

            return true;
        }
    }
}
