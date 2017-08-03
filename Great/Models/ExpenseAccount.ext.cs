using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public partial class ExpenseAccount : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsNew { get { return EStatus == EFDLStatus.New; } } // used for sorting purpose

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
    }
}
