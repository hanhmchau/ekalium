using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class Extra
    {
        [Key]
        public int Id { get; set; }
        public Product Product { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Option> Options { get; set; }
        public bool Optional { get; set; }
        public bool Deleted { get; set; }
    }
}