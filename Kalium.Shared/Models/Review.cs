using System;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public User User { get; set; }
        public Product Product { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Deleted { get; set; }
    }
}