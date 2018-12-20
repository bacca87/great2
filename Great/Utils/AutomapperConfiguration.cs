using AutoMapper;
using Great.Models.Database;
using Great.Models.DTO;
using Great.ViewModels.Database;

namespace Great.Utils
{
    public static class AutomapperConfiguration
    {
        public static void RegisterMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Expense, ExpenseEVM>();
                cfg.CreateMap<ExpenseEVM, Expense>()
                   .ForMember(x => x.ExpenseType, opt => opt.Ignore())
                   .ForMember(x => x.ExpenseAccount1, opt => opt.Ignore());

                cfg.CreateMap<ExpenseAccount, ExpenseAccountEVM>();
                cfg.CreateMap<ExpenseAccountEVM, ExpenseAccount>()
                   .ForMember(x => x.Currency1, opt => opt.Ignore())
                   .ForMember(x => x.Expenses, opt => opt.Ignore())
                   .ForMember(x => x.FDLStatus, opt => opt.Ignore())
                   .ForMember(x => x.FDL1, opt => opt.Ignore());

                cfg.CreateMap<CarRentalHistory, CarRentalHistoryEVM>();
                cfg.CreateMap<CarRentalHistoryEVM, CarRentalHistory>()
                    .ForMember(x => x.Car1, opt => opt.Ignore());

                cfg.CreateMap<Car, CarEVM>();
                cfg.CreateMap<CarEVM, Car>()
                    .ForMember(x => x.CarRentalCompany1, opt => opt.Ignore())
                    .ForMember(x => x.CarRentalHistories, opt => opt.Ignore());

                cfg.CreateMap<FDL, FDLEVM>();
                cfg.CreateMap<FDLEVM, FDL>()
                    .ForMember(x => x.Factory1, opt => opt.Ignore())
                    .ForMember(x => x.FDLStatus, opt => opt.Ignore())
                    .ForMember(x => x.FDLResult, opt => opt.Ignore())
                    .ForMember(x => x.Timesheets, opt => opt.Ignore());

                cfg.CreateMap<Day, DayEVM>();
                cfg.CreateMap<DayEVM, Day>()
                    .ForMember(x => x.DayType, opt => opt.Ignore())
                    .ForMember(x => x.Timesheets, opt => opt.Ignore());

                cfg.CreateMap<Timesheet, TimesheetEVM>();
                    cfg.CreateMap<TimesheetEVM, Timesheet>()
                    .ForMember(x => x.Day, opt => opt.Ignore())
                    .ForMember(x => x.FDL1, opt => opt.Ignore());

                cfg.CreateMap<FDL, FDLDTO>();
                cfg.CreateMap<FDLStatus, FDLStatusDTO>();
                cfg.CreateMap<Currency, CurrencyDTO>();
                cfg.CreateMap<ExpenseType, ExpenseTypeDTO>();
                cfg.CreateMap<Factory, FactoryDTO>();
                cfg.CreateMap<TransferType, TransferTypeDTO>();
                cfg.CreateMap<FDLResult, FDLResultDTO>();
                cfg.CreateMap<Timesheet, TimesheetDTO>();
                cfg.CreateMap<Car, CarDTO>();
                cfg.CreateMap<CarRentalCompany, CarRentalCompanyDTO>();
                cfg.CreateMap<Day, DayDTO>();
                cfg.CreateMap<DayType, DayTypeDTO>();
            });
        }
    }
}
