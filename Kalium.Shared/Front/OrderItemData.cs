using System;
using System.Collections.Generic;
using System.Text;
using Kalium.Shared.Models;

namespace Kalium.Shared.Front
{
    public class OrderItemData
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNameUrl { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public ICollection<Choice> Choices { get; set; }
        public double ActualPrice { get; set; }
    }
}
