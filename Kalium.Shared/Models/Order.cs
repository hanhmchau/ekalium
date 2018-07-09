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
        public string BillingName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public int PaymentMethod { get; set; }
        public int Status { get; set; }
        public bool ShipToDifferentAddress { get; set; }
        public string AlternateName { get; set; }
        public string AlternateAddress { get; set; }
        public string AlternatePhone { get; set; }
        public Refund Refund { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<OrderCoupon> OrderCoupons { get; set; }
        [NotMapped]
        public ICollection<Coupon> Coupons => OrderCoupons.Select(oc => oc.Coupon).ToList();
        public string Note { get; set; }
        [NotMapped]
        public double PreCouponTotal => OrderItems.Sum(orderItem => orderItem.ActualPrice);
        [NotMapped]
        public double PostCouponTotal => OrderItems.Sum(orderItem => orderItem.ActualPrice) - Coupons?.Sum(c => c?.Reduction) ?? 0;
    }
}
