using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using Newtonsoft.Json;

namespace Kalium.Shared.Models
{
    public class Choice
    {
        [JsonProperty("extra")]
        public int Extra { get; set; }
        [JsonProperty("option")]
        public int? Option { get; set; }
    }
    public class PseudoCartItem
    {
        [JsonProperty("guid")]
        public string Guid { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        [JsonProperty("choices")]
        public ICollection<Choice> Choices { get; set; }
    }
    public class PseudoCart
    {
        public ICollection<PseudoCartItem> Cart { get; set; }
        public ICollection<int> Coupons { get; set; }
    }

    [JsonArray]
    public class ExtraDictionary : Dictionary<Extra, Option>
    {
        public ICollection<Choice> ToPseudo() => this.Select(entry => new Choice
        {
            Extra = entry.Key.Id,
            Option = entry.Value?.Id
        }).ToList();
    }
    public class ECartItem
    {
        public string Guid { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public ExtraDictionary Choices { get; set; } = new ExtraDictionary();
        public double Total() => (Product.DiscountedPrice + Choices.Sum(choice => choice.Value?.Price ?? 0)) * Quantity;
        public double SingleTotal() => Product.DiscountedPrice + Choices.Sum(choice => choice.Value?.Price ?? 0);
        public PseudoCartItem ToPseudo() => new PseudoCartItem
        {
            Guid = Guid,
            Id = Product.Id,
            Quantity = Quantity,
            Choices = Choices.ToPseudo()
        };
    }
    public class ECart
    {
        public ICollection<ECartItem> Contents { get; set; } = new List<ECartItem>();
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public ICollection<Coupon> AvailableCoupons =>
            Contents.SelectMany(item => item.Product.Coupons).Distinct().Where(coupon => coupon.IsValid).ToList();
        public double PreCouponTotal() => Contents.Sum(item => item.Total());
        public double PostCouponTotal() => Contents.Sum(item => item.Total()) - Coupons.Sum(coupon => coupon.Reduction);
        public PseudoCart ToPseudo()
        {
            var pseudoCart = new PseudoCart
            {
                Cart = Contents.Select(item => item.ToPseudo()).ToList(),
                Coupons = Coupons.Select(coupon => coupon.Id).Distinct().ToList()
            };
            return pseudoCart;
        }
    }
}
