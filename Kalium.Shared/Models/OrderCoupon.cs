namespace Kalium.Shared.Models
{
    public class OrderCoupon
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }
    }
}