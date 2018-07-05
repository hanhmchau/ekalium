using System;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public DateTime DateExpired { get; set; }
        public double Reduction { get; set; }
    }
}