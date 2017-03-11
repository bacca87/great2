using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models
{
    public partial class ExpenseAccount
    {
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
