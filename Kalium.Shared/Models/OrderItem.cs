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
        public Refund Refund { get; set; }
        public ICollection<OrderItemOption> OrderItemOptions { get; set; }
        [NotMapped]
        public double ActualPrice => Price + ((OrderItemOptions as List<OrderItemOption>)?.Select(oio => oio.Option).Sum(option => option.Price) ?? 0);
    }
}