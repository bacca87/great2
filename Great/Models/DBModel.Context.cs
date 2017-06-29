﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Great.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DBEntities : DbContext
    {
        public DBEntities()
            : base("name=DBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
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
        public virtual DbSet<FDLStatu> FDLStatus { get; set; }
        public virtual DbSet<Timesheet> Timesheets { get; set; }
        public virtual DbSet<TransferType> TransferTypes { get; set; }
    }
}
