using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kalium.Shared.Front;
using Kalium.Shared.Models;
using Choice = Kalium.Shared.Front.Choice;

namespace Kalium.Shared.Conversions
{
    public static class Converter
    {
        public static CouponData Simplify(this Coupon coupon)
        {
            return new CouponData
            {
                Id = coupon.Id,
                Key = coupon.Key,
                ProductId = coupon.Product.Id,
                Type = coupon.Type,
                Quantity = coupon.Quantity,
                DateExpired = coupon.DateExpired,
                Reduction = coupon.Reduction,
                Deleted = coupon.Deleted
            };
        }
        public static Choice Simplify(this OrderItemOption oio)
        {
            return new Choice
            {
                ExtraName = oio.Option?.Extra?.Name,
                OptionName = oio.Option?.Name
            };
        }

        public static OrderItemData Simplify(this OrderItem oi)
        {
            return new OrderItemData
            {
                Id = oi.Id,
                ProductId = oi.Product.Id,
                ProductName = oi.Product.Name,
                ProductNameUrl = oi.Product.NameUrl,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Choices = oi.OrderItemOptions.Select(oio => oio.Simplify()).ToList(),
                ActualPrice = oi.ActualPrice
            };
        }

        public static OrderData Simplify(this Order order)
        {
            return new OrderData
            {
                Id = order.Id,
                BillingName = order.BillingName,
                Address = order.Address,
                AlternateAddress = order.AlternateAddress,
                AlternateName = order.AlternateName,
                AlternatePhone = order.AlternatePhone,
                DateCreated = order.DateCreated,
                PaymentMethod = order.PaymentMethod,
                PhoneNumber = order.PhoneNumber,
                RefundDate = order.Refund?.DateRefunded,
                Status = order.Status,
                ShipToDifferentAddress = order.ShipToDifferentAddress,
                UserId = order.User.Id,
                OrderItems = order.OrderItems.Select(oi => oi.Simplify()).ToList(),
                Coupons = order.Coupons?.Select(c => c.Simplify()).ToList()
            };
        }
    }
}
