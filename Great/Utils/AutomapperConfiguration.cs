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

                cfg.CreateMap<FDL, FDLDTO>();
                cfg.CreateMap<FDLStatus, FDLStatusDTO>();
                cfg.CreateMap<Currency, CurrencyDTO>();
                cfg.CreateMap<ExpenseType, ExpenseTypeDTO>();
                cfg.CreateMap<Factory, FactoryDTO>();
                cfg.CreateMap<TransferType, TransferTypeDTO>();

                cfg.CreateMap<CarRentalHistory, CarRentalHistoryEVM>();
                cfg.CreateMap<CarRentalHistoryEVM, CarRentalHistory>();
                cfg.CreateMap<Car, CarEVM>();
                cfg.CreateMap<CarEVM, Car>();
                cfg.CreateMap<Car, CarDTO>();
                cfg.CreateMap<CarRentalCompany, CarRentalCompanyDTO>();
            });
        }
    }
}
