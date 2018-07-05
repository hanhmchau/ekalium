using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Linq;

namespace Kalium.Shared.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public User User { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public int Status { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        [ForeignKey("CouponId")]
        public Coupon Coupon { get; set; }
        [NotMapped]
        public double Total => (OrderItems as List<OrderItem>).Sum(orderItem => orderItem.ActualPrice * (1 - orderItem?.Refund.RefundRate ?? 0)) - (this?.Coupon.Reduction ?? 0);
    }
}
