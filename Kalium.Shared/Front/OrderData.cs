using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Kalium.Shared.Models;

namespace Kalium.Shared.Front
{
    public class OrderData
    {
        public int Id { get; set; }
        public string UserId { get; set; }
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
        public DateTime? RefundDate { get; set; }
        public ICollection<OrderItemData> OrderItems { get; set; }
        public ICollection<CouponData> Coupons { get; set; }
        public double PreCouponTotal => OrderItems.Sum(orderItem => orderItem.ActualPrice);
        public double PostCouponTotal => OrderItems.Sum(orderItem => orderItem.ActualPrice) - Coupons?.Sum(c => c?.Reduction) ?? 0;
        public bool IsCancellable => (Status == (int)Consts.Consts.OrderStatus.Processing ||
                                      Status == (int)Consts.Consts.OrderStatus.Delivering) && RefundDate == null;
    }
}
