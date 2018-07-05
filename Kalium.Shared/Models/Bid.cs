using System;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }
        public User Bidder { get; set; }
        public double Price { get; set; }
        public DateTime DateBidded { get; set; }
        public Auction Auction { get; set; }
    }
}