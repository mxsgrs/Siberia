using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siberia.CoreWebAPI.Models.NordStreamDb
{
    [Table("Bank")]
    public partial class Bank
    {
        [Key]
        public int SerialId { get; set; }
        [Key]
        public int MarketNoId { get; set; }
        [StringLength(50)]
        public string Company { get; set; } = "";
        [StringLength(50)]
        public string Market { get; set; } = "";
        [StringLength(50)]
        public string Country { get; set; } = "";
    }
}
