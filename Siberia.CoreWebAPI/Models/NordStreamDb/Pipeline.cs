using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siberia.CoreWebAPI.Models.NordStreamDb
{
    [Table("Pipeline")]
    public partial class Pipeline
    {
        [Key]
        public int FirstId { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [Key]
        [StringLength(50)]
        public string LocationId { get; set; } = "";
    }
}
