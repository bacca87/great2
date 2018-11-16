using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.Database
{
    public partial class Expense
    {
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
