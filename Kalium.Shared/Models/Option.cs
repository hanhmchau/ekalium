using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class Option
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public Extra Extra { get; set; }
        public ICollection<OrderItemOption> OrderItemOptions { get; set; }
        public bool Deleted { get; set; }
    }
}