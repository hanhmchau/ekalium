using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Kalium.Shared.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
        [NotMapped]
        public int QuantitySold => Products?.Sum(p => p.QuantitySold) ?? 0;
        [NotMapped]
        public double TotalEarning => Products?.Sum(p => p.TotalEarning) ?? 0;
        [NotMapped]
        public double AverageRating => Products != null && Products.Any() ? Products.Average(p => p.AverageRating) : 0;
    }
}
