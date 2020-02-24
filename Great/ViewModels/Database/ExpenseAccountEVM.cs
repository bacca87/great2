using Great2.Models;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.Models.Interfaces;
using Great2.Utils;
using Great2.Utils.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;

namespace Great2.ViewModels.Database
{
    public class ExpenseAccountEVM : EntityViewModelBase, IFDLFile
    {
        #region Properties
        private long _Id;
        public long Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private string _FDL;
        public string FDL
        {
            get => _FDL;
            set => Set(ref _FDL, value);
        }

        public string Year { get => FDL?.Substring(0, 4);}

        private long? _CdC;
        public long? CdC
        {
            get => _CdC;
            set => Set(ref _CdC, value);
        }

        private string _Currency;
        public string Currency
        {
            get => _Currency;
            set
            {
                SetAndCheckChanged(ref _Currency, value);
                CurrencyCode = CurrencyCodeMapper.GetSymbol(_Currency);
            }
        }

        private double? _DeductionAmount;
        public double? DeductionAmount
        {
            get => _DeductionAmount;
            set
            {
                SetAndCheckChanged(ref _DeductionAmount, value);
                RaisePropertyChanged(nameof(DeductionAmount_Display));
            }
        }

        private string _Notes;
        public string Notes
        {
            get => _Notes;
            set => SetAndCheckChanged(ref _Notes, value);
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
            internal set => Set(ref _IsNew, value);
        }

        private string _CurrencyCode;
        public string CurrencyCode
        {
            get => _CurrencyCode;
            set => Set(ref _CurrencyCode, value);
        }

        public EFDLStatus EStatus
        {
            get => (EFDLStatus)Status;
            set
            {
                Status = (long)value;
                RaisePropertyChanged();
            }
        }

        private DateTime?[] _DaysOfWeek;
        public DateTime?[] DaysOfWeek
        {
            get => _DaysOfWeek;
            set => Set(ref _DaysOfWeek, value);
        }

        public double? MondayAmount => Expenses?.Sum(x => x.MondayAmount);
        public double? TuesdayAmount => Expenses?.Sum(x => x.TuesdayAmount);
        public double? WednesdayAmount => Expenses?.Sum(x => x.WednesdayAmount);
        public double? ThursdayAmount => Expenses?.Sum(x => x.ThursdayAmount);
        public double? FridayAmount => Expenses?.Sum(x => x.FridayAmount);
        public double? SaturdayAmount => Expenses?.Sum(x => x.SaturdayAmount);
        public double? SundayAmount => Expenses?.Sum(x => x.SundayAmount);
        public double? TotalAmount => Expenses?.Sum(x => x.TotalAmount);

        private bool _InsertExpenseEnabled;
        public bool InsertExpenseEnabled
        {
            get => _InsertExpenseEnabled;
            set => Set(ref _InsertExpenseEnabled, value);
        }

        public bool IsExcel => Path.GetExtension(FileName) == ".xlsx";
        #endregion;

        #region Display Properties
        public string Factory_Display => FDL1?.Factory1?.Name;
        public string FDL_New_Display => $"{(NotifyAsNew ? "*" : "")}{FDL}{(IsVirtual ? "(V)" : string.Empty)}";
        public string FDL_Display => $"{FDL}{(IsVirtual ? " (Virtual)" : string.Empty)}";
        public string TotalAmount_Display => TotalAmount > 0 ? $"{TotalAmount}{CurrencyCode}" : string.Empty;
        public string DeductionAmount_Display => DeductionAmount > 0 ? $"{DeductionAmount}{CurrencyCode}" : string.Empty;
        #endregion

        public ExpenseAccountEVM(ExpenseAccount ea = null)
        {
            Expenses = new ObservableCollectionEx<ExpenseEVM>();
            Expenses.CollectionChanged += (sender, e) => UpdateTotals();
            Expenses.CollectionChanged += (sender, e) => InsertExpenseEnabled = Expenses.Count < ApplicationSettings.ExpenseAccount.MaxExpenseCount;
            Expenses.ItemPropertyChanged += (sender, e) => UpdateTotals();

            if (ea != null)
            {
                Auto.Mapper.Map(ea, this);
                InitDaysOfWeek();
            }

            IsChanged = false;
        }

        private void UpdateTotals()
        {
            IsChanged = true;
            RaisePropertyChanged(nameof(MondayAmount));
            RaisePropertyChanged(nameof(TuesdayAmount));
            RaisePropertyChanged(nameof(WednesdayAmount));
            RaisePropertyChanged(nameof(ThursdayAmount));
            RaisePropertyChanged(nameof(FridayAmount));
            RaisePropertyChanged(nameof(SaturdayAmount));
            RaisePropertyChanged(nameof(SundayAmount));
            RaisePropertyChanged(nameof(TotalAmount));

            RaisePropertyChanged(nameof(TotalAmount_Display));
            RaisePropertyChanged(nameof(DeductionAmount_Display));
        }

        private ExpenseEVM CreateExpense(int ExpenseTypeId, DBArchive db = null)
        {
            ExpenseEVM expense = new ExpenseEVM() { ExpenseAccount = Id, Type = ExpenseTypeId };

            if(db != null)
            {
                expense.Save(db);
                expense.Refresh(db);
            }
            else
            {
                using (DBArchive db2 = new DBArchive())
                {
                    expense.Save(db2);
                    expense.Refresh(db2);
                }   
            }
            
            Expenses.Add(expense);

            return expense;
        }

        public void UpdateDiaria(TimesheetEVM timesheet, bool remove, DBArchive db = null)
        {
            if (!UserSettings.Options.AutomaticAllowance || timesheet == null || !FactoryEVM.CheckForfaitCountry(timesheet?.FDL1?.Factory1?.CountryCode))
                return;

            ExpenseEVM expense = Expenses.Where(e => e.Type == ApplicationSettings.ExpenseAccount.DiariaType).FirstOrDefault();

            if (expense == null)
                expense = CreateExpense(IsExcel ? ApplicationSettings.ExpenseAccount.DailyAllowanceType : ApplicationSettings.ExpenseAccount.DiariaType);

            double? Diaria = remove ? (double?)null : ApplicationSettings.ExpenseAccount.DiariaValue;

            if (!remove &&
                (timesheet.TravelPeriods != null && timesheet.WorkPeriods == null && (timesheet.TravelPeriods.Start.TimeOfDay >= ApplicationSettings.ExpenseAccount.DiariaStartThreshold || (timesheet.TravelPeriods.End.Day == timesheet.TravelPeriods.Start.Day && timesheet.TravelPeriods.End.TimeOfDay <= ApplicationSettings.ExpenseAccount.DiariaEndThreshold))) ||
                (timesheet.TravelPeriods != null && timesheet.WorkPeriods != null && timesheet.TravelPeriods.Start < timesheet.WorkPeriods.Start && timesheet.TravelPeriods.Start.TimeOfDay >= ApplicationSettings.ExpenseAccount.DiariaStartThreshold) ||
                (timesheet.TravelPeriods != null && timesheet.WorkPeriods != null && timesheet.TravelPeriods.End > timesheet.WorkPeriods.End && timesheet.TravelPeriods.End.Day == timesheet.TravelPeriods.Start.Day && timesheet.TravelPeriods.End.TimeOfDay <= ApplicationSettings.ExpenseAccount.DiariaEndThreshold))
                Diaria /= 2;

            switch (timesheet.Date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    expense.MondayAmount = Diaria;
                    break;
                case DayOfWeek.Tuesday:
                    expense.TuesdayAmount = Diaria;
                    break;
                case DayOfWeek.Wednesday:
                    expense.WednesdayAmount = Diaria;
                    break;
                case DayOfWeek.Thursday:
                    expense.ThursdayAmount = Diaria;
                    break;
                case DayOfWeek.Friday:
                    expense.FridayAmount = Diaria;
                    break;
                case DayOfWeek.Saturday:
                    expense.SaturdayAmount = Diaria;
                    break;
                case DayOfWeek.Sunday:
                    expense.SundayAmount = Diaria;
                    break;
            }

            if (db != null)
                expense.Save(db);
            else
                expense.Save();

            IsChanged = false;
        }

        public void UpdatePocketMoney(TimesheetEVM timesheet, bool remove, DBArchive db = null)
        {
            if (!UserSettings.Options.AutomaticAllowance || timesheet == null || FactoryEVM.CheckForfaitCountry(timesheet?.FDL1?.Factory1?.CountryCode))
                return;

            ExpenseEVM expense = Expenses.Where(e => e.Type == ApplicationSettings.ExpenseAccount.PocketMoneyType).FirstOrDefault();

            if (expense == null)
                expense = CreateExpense(IsExcel ? ApplicationSettings.ExpenseAccount.PocketMoney1Type : ApplicationSettings.ExpenseAccount.PocketMoneyType);

            double? PocketMoney = remove ? (double?)null : ApplicationSettings.ExpenseAccount.PocketMoneyValue;

            switch (timesheet.Date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    expense.MondayAmount = PocketMoney;
                    break;
                case DayOfWeek.Tuesday:
                    expense.TuesdayAmount = PocketMoney;
                    break;
                case DayOfWeek.Wednesday:
                    expense.WednesdayAmount = PocketMoney;
                    break;
                case DayOfWeek.Thursday:
                    expense.ThursdayAmount = PocketMoney;
                    break;
                case DayOfWeek.Friday:
                    expense.FridayAmount = PocketMoney;
                    break;
                case DayOfWeek.Saturday:
                    expense.SaturdayAmount = PocketMoney;
                    break;
                case DayOfWeek.Sunday:
                    expense.SundayAmount = PocketMoney;
                    break;
            }

            if (db != null)
                expense.Save(db);
            else
                expense.Save();

            IsChanged = false;
        }

        public void UpdateDiariaAndPocketMoney(ObservableCollection<TimesheetEVM> timesheets)
        {
            if (!UserSettings.Options.AutomaticAllowance || timesheets == null)
                return;

            foreach(var timesheet in timesheets)
            {
                UpdateDiaria(timesheet, false);
                UpdatePocketMoney(timesheet, false);
            }
        }

        public void InitDaysOfWeek()
        {
            DateTime StartDay = DateTime.Now.FromUnixTimestamp(FDL1.StartDay);
            DateTime StartDayOfWeek = StartDay.AddDays((int)DayOfWeek.Monday - (int)StartDay.DayOfWeek);
            var Days = Enumerable.Range(0, 7).Select(i => StartDayOfWeek.AddDays(i)).ToArray();

            DateTime?[] tmpDays = new DateTime?[7];

            for (int i = 0; i < 7; i++)
                tmpDays[i] = Days[i].Month == StartDay.Month ? Days[i] : (DateTime?)null;

            DaysOfWeek = tmpDays;
        }

        public void InitExpenses(DBArchive db = null)
        {
            if(IsExcel)
            {
                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.LunchType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.LunchType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.DinnerType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.DinnerType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.FuelType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.FuelType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.HotelType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.HotelType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.TollType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.TollType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.ParkingType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.ParkingType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.Taxi1Type))
                    CreateExpense(ApplicationSettings.ExpenseAccount.Taxi1Type, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.ExtraBaggageType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.ExtraBaggageType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.CurrencyTransactionFeesType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.CurrencyTransactionFeesType, db);
            }
            else
            {
                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.PranzoType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.PranzoType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.CenaType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.CenaType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.CarburanteEsteroType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.CarburanteEsteroType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.CarburanteItaliaType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.CarburanteItaliaType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.PedaggiType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.PedaggiType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.ParcheggioType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.ParcheggioType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.TaxiType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.TaxiType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.ExtraBagaglioType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.ExtraBagaglioType, db);

                if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.CommissioniValutaType))
                    CreateExpense(ApplicationSettings.ExpenseAccount.CommissioniValutaType, db);
            }

            IsChanged = false;
        }

        public void InitExpensesByCountry(DBArchive db = null)
        {
            if (FDL1 == null || FDL1.Factory1 == null || FDL1.Factory1.CountryCode == null || FDL1.Factory1.CountryCode == string.Empty)
                return;

            string CountryCode = FDL1.Factory1.CountryCode;

            if (!IsExcel)
            {
                if (CountryCode == "IT")
                {
                    if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.HotelItaliaType))
                        CreateExpense(ApplicationSettings.ExpenseAccount.HotelItaliaType, db);
                }
                else
                {
                    if (!Expenses.Any(e => e.Type == ApplicationSettings.ExpenseAccount.HotelEsteroType))
                        CreateExpense(ApplicationSettings.ExpenseAccount.HotelEsteroType, db);
                }
            }

            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            ExpenseAccount ea = new ExpenseAccount();

            Auto.Mapper.Map(this, ea);
            db.ExpenseAccounts.AddOrUpdate(ea);
            db.SaveChanges();
            Id = ea.Id;
            IsChanged = false;
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            throw new System.NotImplementedException();
        }

        public override bool Refresh(DBArchive db)
        {
            var exp = db.ExpenseAccounts.SingleOrDefault(x => x.Id == Id);

            if (exp != null)
            {
                db.Entry(exp).Reference(p => p.FDL1).Load();
                db.Entry(exp).Reference(p => p.Currency1).Load();

                Auto.Mapper.Map(exp, this);
                IsChanged = false;
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExpenseAccountEVM)
            {
                ExpenseAccountEVM exp = obj as ExpenseAccountEVM;
                return Id == exp.Id &&
                       FDL == exp.FDL &&
                       FileName == exp.FileName &&
                       Currency == exp.Currency &&
                       Notes == exp.Notes &&
                       Currency == exp.Currency &&
                       Expenses.SequenceEqual(exp.Expenses);
            }
            return false;
        }

        public override int GetHashCode()
        {
            //Override needed only for dictionaries 
            //https://www.codeproject.com/Tips/1255596/Overriding-Equals-GetHashCode-Laconically-in-CShar

            return base.GetHashCode();
        }
    }
}
