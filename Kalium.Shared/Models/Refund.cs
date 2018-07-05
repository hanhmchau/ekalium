using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kalium.Shared.Models
{
    public class Refund
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("OrderItemId")]
        public OrderItem OrderItem { get; set; }
        public DateTime DateRefunded { get; set; }
        public double RefundRate { get; set; }
    }
}