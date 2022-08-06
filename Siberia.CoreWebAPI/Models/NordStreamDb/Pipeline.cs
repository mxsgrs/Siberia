using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siberia.CoreWebAPI.Models.NordStreamDb
{
    [Table("Pipeline")]
    public partial class Pipeline
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string? Company { get; set; }
        [StringLength(50)]
        public string MainLocation { get; set; } = "";
    }
}
