using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kalium.Shared.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public Product Product { get; set; }
        public int Type { get; set; }
        public int Quantity { get; set; }
        public DateTime? DateExpired { get; set; }
        public double Reduction { get; set; }
        [NotMapped]
        public bool IsValid =>
            Type == (int) Consts.Consts.CouponType.Date ? DateTime.Now < DateExpired : Quantity > 0;

        public bool Deleted { get; set; }
    }
}