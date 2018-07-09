using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Kalium.Shared.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameUrl { get; set; }
        public string Format { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }
        public string Process { get; set; }
        public string Features { get; set; }
        public int PageCount { get; set; }
        public string Origin { get; set; }
        public Brand Brand { get; set; }
        public int Status { get; set; }
        public int ReviewStatus { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public User Creator { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public bool Featured { get; set; }
        public double Price { get; set; }
        public double DiscountedPrice { get; set; }
        public DateTime DateCreated { get; set; }
        public ICollection<Image> Images { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Coupon> Coupons { get; set; }
        public ICollection<Extra> Extras { get; set; }
        public ICollection<Discussion> Discussions { get; set; }
        [NotMapped]
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Auction> Auctions { get; set; }
        [NotMapped]
        public double AverageRating => Reviews?.Where(rev => !rev.Deleted).Average(rev => rev.Rating) ?? 0;
        [NotMapped]
        public int QuantitySold => OrderItems?.Where(oi => oi.Order.Refund == null).Sum(oi => oi.Quantity) ?? 0;
        [NotMapped]
        public double TotalEarning => OrderItems?.Sum(oi => oi.PriceAfterRefund) ?? 0;
        [NotMapped]
        public bool IsOnSale => DiscountedPrice.CompareTo(Price) != 0;
        [NotMapped] public Image MainImage => Images?.DefaultIfEmpty(Consts.Consts.DefaultImage).FirstOrDefault();
    }
}
