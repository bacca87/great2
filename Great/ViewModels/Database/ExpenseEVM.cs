
using Great2.Models.Database;
using Great2.Models.DTO;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Great2.ViewModels.Database
{
    public class ExpenseEVM : EntityViewModelBase
    {
        #region Properties
        public long _Id;
        public long Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        public long _ExpenseAccount;
        public long ExpenseAccount
        {
            get => _ExpenseAccount;
            set => Set(ref _ExpenseAccount, value);
        }

        private long _Type;
        public long Type
        {
            get => _Type;
            set => SetAndCheckChanged(ref _Type, value);
        }

        private double? _MondayAmount;
        public double? MondayAmount
        {
            get => _MondayAmount;
            set
            {
                SetAndCheckChanged(ref _MondayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _TuesdayAmount;
        public double? TuesdayAmount
        {
            get => _TuesdayAmount;
            set
            {
                SetAndCheckChanged(ref _TuesdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _WednesdayAmount;
        public double? WednesdayAmount
        {
            get => _WednesdayAmount;
            set
            {
                SetAndCheckChanged(ref _WednesdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));

            }
        }

        private double? _ThursdayAmount;
        public double? ThursdayAmount
        {
            get => _ThursdayAmount;
            set
            {
                SetAndCheckChanged(ref _ThursdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _FridayAmount;
        public double? FridayAmount
        {
            get => _FridayAmount;
            set
            {
                SetAndCheckChanged(ref _FridayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _SaturdayAmount;
        public double? SaturdayAmount
        {
            get => _SaturdayAmount;
            set
            {
                SetAndCheckChanged(ref _SaturdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _SundayAmount;
        public double? SundayAmount
        {
            get => _SundayAmount;
            set
            {
                SetAndCheckChanged(ref _SundayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        public double TotalAmount => (MondayAmount ?? 0) + (TuesdayAmount ?? 0) + (WednesdayAmount ?? 0) + (ThursdayAmount ?? 0) + (FridayAmount ?? 0) + (SaturdayAmount ?? 0) + (SundayAmount ?? 0);

        public ExpenseTypeDTO _ExpenseType;
        public ExpenseTypeDTO ExpenseType
        {
            get => _ExpenseType;
            set => Set(ref _ExpenseType, value);
        }
        #endregion

        // hack because XAML didnt support default parameters
        public ExpenseEVM() { }

        public ExpenseEVM(Expense expense = null)
        {
            if (expense != null)
                Auto.Mapper.Map(expense, this);
            IsChanged = false;
        }

        public override bool Save(DBArchive db)
        {
            Expense e = new Expense();

            Auto.Mapper.Map(this, e);
            db.Expenses.AddOrUpdate(e);
            db.SaveChanges();
            Id = e.Id;
            Refresh(db);
            IsChanged = false;            
            return true;
        }

        public override bool Delete(DBArchive db)
        {
            throw new System.NotImplementedException();
        }

        public override bool Refresh(DBArchive db)
        {
            Expense expense = db.Expenses.SingleOrDefault(e => e.Id == Id);

            if (expense != null)
            { 
                Auto.Mapper.Map(expense, this);
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExpenseEVM)
            {
                ExpenseEVM exp = obj as ExpenseEVM;

                return Id == exp.Id &&
                       ExpenseAccount == exp.ExpenseAccount &&                       
                       MondayAmount == exp.MondayAmount &&
                       TuesdayAmount == exp.TuesdayAmount &&
                       ThursdayAmount == exp.ThursdayAmount &&
                       WednesdayAmount == exp.WednesdayAmount &&
                       FridayAmount == exp.FridayAmount &&
                       SaturdayAmount == exp.SaturdayAmount &&
                       SundayAmount == exp.SundayAmount;
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
