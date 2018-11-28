using AutoMapper;
using GalaSoft.MvvmLight;
using Great.Models;
using Great.Models.Database;
using Great.Models.DTO;
using Great.Models.Interfaces;
using Great.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.ViewModels.Database
{
    public class ExpenseAccountEVM : ViewModelBase, IFDLFile
    {
        #region Properties
        public long Id { get; set; }
        public string FDL { get; set; }
        public long? CdC { get; set; }

        private string _Currency;
        public string Currency
        {
            get => _Currency;
            set
            {
                Set(ref _Currency, value);
                CurrencyCode = CurrencyCodeMapper.GetSymbol(_Currency);
            }
        }

        private string _Notes;
        public string Notes
        {
            get => _Notes;
            set => Set(ref _Notes, value);
        }

        private long _Status;
        public long Status
        {
            get => _Status;
            set => Set(ref _Status, value);
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

        private bool _IsRefunded;
        public bool IsRefunded
        {
            get => _IsRefunded;
            set => Set(ref _IsRefunded, value);
        }

        private bool _NotifyAsNew;
        public bool NotifyAsNew
        {
            get => _NotifyAsNew;
            set => Set(ref _NotifyAsNew, value);
        }

        private CurrencyDTO _Currency1;
        public CurrencyDTO Currency1
        {
            get => _Currency1;
            set => Set(ref _Currency1, value);
        }

        private ObservableCollectionEx<ExpenseEVM> _Expenses;
        public ObservableCollectionEx<ExpenseEVM> Expenses
        {
            get => _Expenses;
            set => Set(ref _Expenses, value);
        }

        private FDLStatusDTO _FDLStatus;
        public FDLStatusDTO FDLStatus
        {
            get => _FDLStatus;
            set => Set(ref _FDLStatus, value);
        }

        private FDLDTO _FDL1;
        public FDLDTO FDL1
        {
            get => _FDL1;
            set => Set(ref _FDL1, value);
        }

        public string FilePath => ApplicationSettings.Directories.ExpenseAccount + FileName;

        private bool _IsNew;
        public bool IsNew // used for sorting purpose
        {
            get => _IsNew;
            set => Set(ref _IsNew, value);
        }

        private string _CurrencyCode;
        public string CurrencyCode
        {
            get => _CurrencyCode;
            set => Set(ref _CurrencyCode, value);
        }

        private EFDLStatus _EStatus;
        public EFDLStatus EStatus
        {
            get => _EStatus;
            set
            {
                Set(ref _EStatus, value);
                IsNew = _EStatus == EFDLStatus.New;
            }
        }

        public double? MondayAmount => Expenses?.Sum(x => x.MondayAmount);
        public double? TuesdayAmount => Expenses?.Sum(x => x.TuesdayAmount);
        public double? WednesdayAmount => Expenses?.Sum(x => x.WednesdayAmount);
        public double? ThursdayAmount => Expenses?.Sum(x => x.ThursdayAmount);
        public double? FridayAmount => Expenses?.Sum(x => x.FridayAmount);
        public double? SaturdayAmount => Expenses?.Sum(x => x.SaturdayAmount);
        public double? SundayAmount => Expenses?.Sum(x => x.SundayAmount);
        public double? TotalAmount => Expenses?.Sum(x => x.TotalAmount);
        #endregion;

        public ExpenseAccountEVM(ExpenseAccount ea)
        {
            Expenses = new ObservableCollectionEx<ExpenseEVM>();
            Expenses.CollectionChanged += Expenses_CollectionChanged;

            Mapper.Map(ea, this);
        }

        private void Expenses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(MondayAmount));
            RaisePropertyChanged(nameof(TuesdayAmount));
            RaisePropertyChanged(nameof(WednesdayAmount));
            RaisePropertyChanged(nameof(ThursdayAmount));
            RaisePropertyChanged(nameof(FridayAmount));
            RaisePropertyChanged(nameof(SaturdayAmount));
            RaisePropertyChanged(nameof(SundayAmount));
            RaisePropertyChanged(nameof(TotalAmount));
        }
    }
}
