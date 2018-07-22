using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kalium.Shared.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool Status { get; set; } = true;
        public ICollection<Product> Products { get; set; }
        [NotMapped]
        public int ProductCount { get; set; }
    }
}
