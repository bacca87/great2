using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Great.Models.Database
{
    public partial class DBArchive : DbContext
    {
        public DBArchive()
            : base("name=DBArchive")
        {
        }

        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<CarRentalCompany> CarRentalCompanies { get; set; }
        public virtual DbSet<CarRentalHistory> CarRentalHistories { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Day> Days { get; set; }
        public virtual DbSet<DayType> DayTypes { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<ExpenseAccount> ExpenseAccounts { get; set; }
        public virtual DbSet<ExpenseType> ExpenseTypes { get; set; }
        public virtual DbSet<Factory> Factories { get; set; }
        public virtual DbSet<FDL> FDLs { get; set; }
        public virtual DbSet<FDLResult> FDLResults { get; set; }
        public virtual DbSet<FDLStatus> FDLStatus { get; set; }
        public virtual DbSet<OrderEmailRecipient> OrderEmailRecipients { get; set; }
        public virtual DbSet<Timesheet> Timesheets { get; set; }
        public virtual DbSet<TransferType> TransferTypes { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<EventType> EventTypes { get; set; }
        public virtual DbSet<EventStatus> EventStatus { get; set; }
    }
}
