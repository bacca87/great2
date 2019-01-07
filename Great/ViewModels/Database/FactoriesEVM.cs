using AutoMapper;
using Great.Models.Database;
using Great.Models.DTO;
using System.Data.Entity.Migrations;

namespace Great.ViewModels.Database
{
    public class FactoryEVM : EntityViewModelBase
    {

        #region Properties

        public long Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long TransferType { get; set; }
        public bool IsForfait { get; set; }
        public bool NotifyAsNew { get; set; }

        public virtual TransferTypeDTO TransferType1 { get; set; }
        #endregion

        public FactoryEVM() { }

        public FactoryEVM(Factory factory)
        {
            Mapper.Map(factory, this);
        }

        public override bool Save(DBArchive db)
        {
            Factory f = new Factory();

            Mapper.Map(this, f);
            db.Factories.AddOrUpdate(f);

            return true;
        }
    }
}
