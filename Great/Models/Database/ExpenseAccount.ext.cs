using Great.Models.Interfaces;
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
