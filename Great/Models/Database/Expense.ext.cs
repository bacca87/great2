using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.Database
{
    public partial class Expense
    {
        [NotMapped]
        public double TotalAmount => (MondayAmount ?? 0) + (TuesdayAmount ?? 0) + (WednesdayAmount ?? 0) + (ThursdayAmount ?? 0) + (FridayAmount ?? 0) + (SaturdayAmount ?? 0) + (SundayAmount ?? 0);

        public Expense Clone()
        {
            return new Expense()
            {
                Id = Id,
                ExpenseAccount = ExpenseAccount,
                Type = Type,
                MondayAmount = MondayAmount,
                TuesdayAmount = TuesdayAmount,
                WednesdayAmount = WednesdayAmount,
                ThursdayAmount = ThursdayAmount,
                FridayAmount = FridayAmount,
                SaturdayAmount = SaturdayAmount,
                SundayAmount = SundayAmount
            };
        }
    }
}
