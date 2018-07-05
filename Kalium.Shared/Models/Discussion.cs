using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class Discussion
    {
        [Key]
        public int Id { get; set; }
        public User Author { get; set; }
        public Product Product { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public Discussion Parent { get; set; }
        public ICollection<Discussion> Replies { get; set; }
    }
}