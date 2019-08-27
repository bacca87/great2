using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("Factory")]
    public partial class Factory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long TransferType { get; set; }
        public bool IsForfait { get; set; }
        public bool NotifyAsNew { get; set; }
        public bool OverrideAddressOnFDL { get; set; }

        [ForeignKey("TransferType")]
        public virtual TransferType TransferType1 { get; set; }
    }
}
