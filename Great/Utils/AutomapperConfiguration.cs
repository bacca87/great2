using AutoMapper;
using Great2.Models.Database;
using Great2.Models.DTO;
using Great2.ViewModels.Database;

namespace Great2
{
    public static class Auto
    {
        public static IMapper Mapper { get; internal set; }

        static Auto()
        {
            var config = new MapperConfiguration(cfg =>
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
                    .ForMember(x => x.CarRentalHistories, opt => opt.Ignore());

                cfg.CreateMap<FDL, FDLEVM>();
                cfg.CreateMap<FDLEVM, FDL>()
                    .ForMember(x => x.Factory1, opt => opt.Ignore())
                    .ForMember(x => x.FDLStatus, opt => opt.Ignore())
                    .ForMember(x => x.FDLResult, opt => opt.Ignore())
                    .ForMember(x => x.Timesheets, opt => opt.Ignore());

                cfg.CreateMap<Event, EventEVM>()
                .ForMember(x => x.Status1, opt => opt.Ignore());
                cfg.CreateMap<EventEVM, Event>()
                    .ForMember(x => x.Status1, opt => opt.Ignore())
                    .ForMember(x => x.Type1, opt => opt.Ignore());

                cfg.CreateMap<EventEVM, EventEVM>();

                cfg.CreateMap<DayEvent, DayEventEVM>()
                    .ForMember(x => x.Event1, opt => opt.Ignore())
                    .ForMember(x => x.Day1, opt => opt.Ignore());

                cfg.CreateMap<DayEventEVM, DayEvent>()
                    .ForMember(x => x.Event1, opt => opt.Ignore())
                    .ForMember(x => x.Day1, opt => opt.Ignore());


                cfg.CreateMap<DayEVM, DayEVM>();
                cfg.CreateMap<Day, DayEVM>();
                cfg.CreateMap<DayEVM, Day>()
                    .ForMember(x => x.DayType, opt => opt.Ignore())
                    //.ForMember(x => x.Event, opt => opt.Ignore())
                    .ForMember(x => x.Timesheets, opt => opt.Ignore());

                cfg.CreateMap<Timesheet, TimesheetEVM>();
                cfg.CreateMap<TimesheetEVM, Timesheet>()
                    .ForMember(x => x.Day, opt => opt.Ignore())
                    .ForMember(x => x.FDL1, opt => opt.Ignore());
                cfg.CreateMap<TimesheetEVM, TimesheetEVM>(); // used for copy paste timesheets

                cfg.CreateMap<Factory, FactoryEVM>();
                cfg.CreateMap<FactoryEVM, Factory>()
                    .ForMember(x => x.TransferType1, opt => opt.Ignore());

                cfg.CreateMap<FDL, FDLDTO>();
                cfg.CreateMap<FDLStatus, FDLStatusDTO>();
                cfg.CreateMap<Currency, CurrencyDTO>();
                cfg.CreateMap<ExpenseType, ExpenseTypeDTO>();
                cfg.CreateMap<ExpenseType, ExpenseTypeEVM>();
                cfg.CreateMap<Factory, FactoryDTO>();
                cfg.CreateMap<TransferType, TransferTypeDTO>();
                cfg.CreateMap<FDLResult, FDLResultDTO>();
                cfg.CreateMap<Timesheet, TimesheetDTO>();
                cfg.CreateMap<Car, CarDTO>();
                cfg.CreateMap<Day, DayDTO>();
                cfg.CreateMap<DayType, DayTypeDTO>();
                cfg.CreateMap<FactoryEVM, FactoryDTO>();
                cfg.CreateMap<EventType, EventTypeDTO>();
                cfg.CreateMap<Event, EventDTO>().ForMember(x => x.Status1, opt => opt.Ignore());
            });

            Mapper = config.CreateMapper();
        }
    }
}
