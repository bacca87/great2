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
                cfg.CreateMap<ExpenseAccount, ExpenseAccountEVM>();

                cfg.CreateMap<FDL, FDLDTO>();
                cfg.CreateMap<FDLStatus, FDLStatusDTO>();
                cfg.CreateMap<Currency, CurrencyDTO>();
                cfg.CreateMap<ExpenseType, ExpenseTypeDTO>();
            });
        }
    }
}
