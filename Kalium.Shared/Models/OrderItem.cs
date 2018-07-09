using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kalium.Shared.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public ICollection<OrderItemOption> OrderItemOptions { get; set; }
        [NotMapped]
        public double ActualPrice => Price + (OrderItemOptions?.Select(oio => oio.Option).Sum(option => option.Price) ?? 0);
        [NotMapped]
        public double PriceAfterRefund => ActualPrice * (1 - Order.Refund?.RefundRate ?? 0);
        [NotMapped]
        public ICollection<Option> Options => OrderItemOptions?.Select(oio => oio.Option).ToList();
    }
}