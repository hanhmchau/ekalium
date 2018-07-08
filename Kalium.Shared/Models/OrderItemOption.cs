using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kalium.Shared.Models
{
    public class OrderItemOption
    {
        public int OptionId { get; set; }
        public Option Option { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }
    }
}
