
using AutoMapper;
using GalaSoft.MvvmLight;
using Great.Models.Database;
using Great.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.ViewModels.Database
{
    public class ExpenseEVM : EntityViewModelBase
    {
        #region Properties
        public long Id { get; set; }

        public long ExpenseAccount { get; set; }

        private long _Type;
        public long Type
        {
            get => _Type;
            set => Set(ref _Type, value);
        }

        private double? _MondayAmount;
        public double? MondayAmount
        {
            get => _MondayAmount;
            set
            {
                Set(ref _MondayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _TuesdayAmount;
        public double? TuesdayAmount
        {
            get => _TuesdayAmount;
            set
            {
                Set(ref _TuesdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _WednesdayAmount;
        public double? WednesdayAmount
        {
            get => _WednesdayAmount;
            set
            {
                Set(ref _WednesdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _ThursdayAmount;
        public double? ThursdayAmount
        {
            get => _ThursdayAmount;
            set
            {
                Set(ref _ThursdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _FridayAmount;
        public double? FridayAmount
        {
            get => _FridayAmount;
            set
            {
                Set(ref _FridayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _SaturdayAmount;
        public double? SaturdayAmount
        {
            get => _SaturdayAmount;
            set
            {
                Set(ref _SaturdayAmount, value);
                RaisePropertyChanged(nameof(TotalAmount));
            }
        }

        private double? _SundayAmount;
        public double? SundayAmount
        {
            get => _SundayAmount;
            set
            {
                Set(ref _SundayAmount, value);
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

        public ExpenseEVM() { }

        public ExpenseEVM(Expense expense)
        {
            Mapper.Map(expense, this);
        }

        public override bool Save(DBArchive db)
        {
            Expense e = new Expense();

            Mapper.Map(this, e);
            db.Expenses.AddOrUpdate(e);

            return true;
        }
    }
}
