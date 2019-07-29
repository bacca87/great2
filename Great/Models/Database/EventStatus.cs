using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Great.Models.Database
{
    [Table("EventStatus")]
    public partial class EventStatus
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
