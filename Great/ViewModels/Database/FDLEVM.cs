﻿using Great2.Models;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Models.Interfaces;
using Great2.Utils.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great2.ViewModels.Database
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

        public DateTime StartDayDate => DateTime.Now.FromUnixTimestamp(_StartDay);

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
            set => SetAndCheckChanged(ref _Factory, value);
        }

        private bool _OutwardCar;
        public bool OutwardCar
        {
            get => _OutwardCar;
            set => SetAndCheckChanged(ref _OutwardCar, value);
        }

        private bool _ReturnCar;
        public bool ReturnCar
        {
            get => _ReturnCar;
            set => SetAndCheckChanged(ref _ReturnCar, value);
        }

        private bool _OutwardTaxi;
        public bool OutwardTaxi
        {
            get => _OutwardTaxi;
            set => SetAndCheckChanged(ref _OutwardTaxi, value);
        }

        private bool _ReturnTaxi;
        public bool ReturnTaxi
        {
            get => _ReturnTaxi;
            set => SetAndCheckChanged(ref _ReturnTaxi, value);
        }

        private bool _OutwardAircraft;
        public bool OutwardAircraft
        {
            get => _OutwardAircraft;
            set => SetAndCheckChanged(ref _OutwardAircraft, value);
        }

        private bool _ReturnAircraft;
        public bool ReturnAircraft
        {
            get => _ReturnAircraft;
            set => SetAndCheckChanged(ref _ReturnAircraft, value);
        }

        private string _PerformanceDescription;
        public string PerformanceDescription
        {
            get => _PerformanceDescription;
            set => SetAndCheckChanged(ref _PerformanceDescription, value);
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
            set => SetAndCheckChanged(ref _ResultNotes, value);
        }

        private string _Notes;
        public string Notes
        {
            get => _Notes;
            set => SetAndCheckChanged(ref _Notes, value);
        }

        private string _PerformanceDescriptionDetails;
        public string PerformanceDescriptionDetails
        {
            get => _PerformanceDescriptionDetails;
            set => SetAndCheckChanged(ref _PerformanceDescriptionDetails, value);
        }

        private long _Status;
        public long Status
        {
            get => _Status;
            set
            {
                Set(ref _Status, value);
                IsNew = _Status == 0;
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
            set
            {
                Set(ref _NotifyAsNew, value);
                RaisePropertyChanged(nameof(FDL_New_Display));
            }
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

        private long? _LastSAPSendTimestamp;
        public long? LastSAPSendTimestamp
        {
            get => _LastSAPSendTimestamp;
            set => Set(ref _LastSAPSendTimestamp, value);
        }

        private bool _IsVirtual;
        public bool IsVirtual
        {
            get => _IsVirtual;
            set => Set(ref _IsVirtual, value);
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

        public ObservableCollection<TimesheetEVM> _Timesheets;
        public ObservableCollection<TimesheetEVM> Timesheets
        {
            get => _Timesheets;
            set => Set(ref _Timesheets, value);
        }

        public string FilePath => ApplicationSettings.Directories.FDL + FileName;

        private bool _IsNew;
        public bool IsNew // used for sorting purpose
        {
            get => _IsNew;
            internal set => Set(ref _IsNew, value);
        }

        public bool IsValid => Timesheets.Count == 0 || Timesheets.All(ts => ts.IsValid);

        public EFDLStatus EStatus
        {
            get => (EFDLStatus)Status;
            set
            {
                Status = (long)value;                
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
        public string FDL_New_Display => $"{(NotifyAsNew ? "*" : "")}{Id}{(IsVirtual ? "(V)" : string.Empty)}";
        public string FDL_Display => $"{Id}{(IsExtra ? " (EXTRA)" : "")}{(IsVirtual ? " (Virtual)" : string.Empty)}";
        public string FDL_Factory_Display => $"{Id}{(Factory1 != null ? $" [{Factory1.Name}]" : "")}{(IsExtra ? " (EXTRA)" : "")}";
        #endregion

        // hack because XAML didnt support default parameters
        public FDLEVM() => Timesheets = new ObservableCollection<TimesheetEVM>();

        public FDLEVM(FDL fdl = null)
        {
            Timesheets = new ObservableCollection<TimesheetEVM>();

            if (fdl != null)
            {
                Auto.Mapper.Map(fdl, this);
                Timesheets = new ObservableCollection<TimesheetEVM>(Timesheets.OrderBy(t => t.Timestamp));
            }   

            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            FDL fdl = new FDL();

            Auto.Mapper.Map(this, fdl);
            db.FDLs.AddOrUpdate(fdl);
            db.SaveChanges();
            IsChanged = false;
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            throw new NotImplementedException();
        }

        public override bool Refresh(DBArchive db)
        {
            FDL fdl = db.FDLs.SingleOrDefault(f => f.Id == Id);

            if (fdl != null)
            {
                Auto.Mapper.Map(fdl, this);

                Timesheets = new ObservableCollection<TimesheetEVM>(Timesheets.OrderBy(t => t.Timestamp));
                return true;
            }

            return false;
        }
    }
}
