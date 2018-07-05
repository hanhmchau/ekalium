using MoreLinq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kalium.Shared.Models
{
    public class Auction
    {
        [Key]
        public int Id { get; set; }
        public Product Product { get; set; }
        public double StartPrice { get; set; }
        public double MinimumSellPrice { get; set; }
        public DateTime StartTime { get; set; }
        public int MinuteToWait { get; set; }
        public ICollection<Bid> Bids { get; set; }
        public int Status { get; set; }
        public Bid WinningBid => Bids.MaxBy(bid => bid.Price).FirstOrDefault(null);
    }
}