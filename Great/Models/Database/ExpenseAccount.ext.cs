using Great.Models.Interfaces;
using Great.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.Database
{
    public partial class ExpenseAccount : INotifyPropertyChanged, IFDLFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotMapped]
        public string FilePath { get { return ApplicationSettings.Directories.ExpenseAccount + FileName; } }
        [NotMapped]
        public bool IsNew { get { return EStatus == EFDLStatus.New; } } // used for sorting purpose

        [NotMapped]
        public string CurrencyCode { get { return CurrencyCodeMapper.GetSymbol(Currency1?.Id); } }

        [NotMapped]
        public EFDLStatus EStatus
        {
            get
            {
                return (EFDLStatus)Status;
            }

            set
            {
                Status = (long)value;
            }
        }

        #region Totals
        [NotMapped]
        public double? MondayAmount => Expenses?.Sum(x => x.MondayAmount);
        [NotMapped]
        public double? TuesdayAmount => Expenses?.Sum(x => x.TuesdayAmount);
        [NotMapped]
        public double? WednesdayAmount => Expenses?.Sum(x => x.WednesdayAmount);
        [NotMapped]
        public double? ThursdayAmount => Expenses?.Sum(x => x.ThursdayAmount);
        [NotMapped]
        public double? FridayAmount => Expenses?.Sum(x => x.FridayAmount);
        [NotMapped]
        public double? SaturdayAmount => Expenses?.Sum(x => x.SaturdayAmount);
        [NotMapped]
        public double? SundayAmount => Expenses?.Sum(x => x.SundayAmount);
        [NotMapped]
        public double? TotalAmount => Expenses?.Sum(x => x.TotalAmount);
        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyFDLPropertiesChanged()
        {
            OnPropertyChanged(nameof(CdC));
            OnPropertyChanged(nameof(Currency));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(LastError));
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(NotifyAsNew));
            OnPropertyChanged(nameof(Currency1));
            OnPropertyChanged(nameof(Expenses));
            OnPropertyChanged(nameof(FDLStatus));
            OnPropertyChanged(nameof(FDL1));
            OnPropertyChanged(nameof(IsRefunded));
            OnPropertyChanged(nameof(CurrencyCode));

            OnPropertyChanged(nameof(MondayAmount));
            OnPropertyChanged(nameof(TuesdayAmount));
            OnPropertyChanged(nameof(WednesdayAmount));
            OnPropertyChanged(nameof(ThursdayAmount));
            OnPropertyChanged(nameof(FridayAmount));
            OnPropertyChanged(nameof(SaturdayAmount));
            OnPropertyChanged(nameof(SundayAmount));
            OnPropertyChanged(nameof(TotalAmount));
        }

        public ExpenseAccount Clone()
        {
            return new ExpenseAccount()
            {
                Id = Id,
                FDL = FDL,
                CdC = CdC,
                Currency = Currency,
                Notes = Notes,
                Status = Status,
                LastError = LastError,
                FileName = FileName,
                NotifyAsNew = NotifyAsNew
            };
        }
    }
}
