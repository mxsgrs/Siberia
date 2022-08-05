﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siberia.CoreWebAPI.Models.NordStreamDb
{
    [Table("Society")]
    public partial class Society
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [StringLength(50)]
        public string? Country { get; set; }
    }
}
