using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Siberia.CoreWebAPI.Models.NordStreamDb
{
    [Table("Pipeline")]
    public partial class Pipeline
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [StringLength(50)]
        public string? Location { get; set; }
    }
}
