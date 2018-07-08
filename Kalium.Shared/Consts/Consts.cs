using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kalium.Shared.Consts
{
    public static class ValidatorUtils
    {
        public static bool IsValidEmail(string email) => Regex.IsMatch(email,
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
            RegexOptions.IgnoreCase);

        public static bool IsValidPhone(string phone) => Regex.IsMatch(phone, @"^(\+[0-9]{8-14})$");
    }
    public static class EnumExtensions
    {
        public static string Name(this Enum en) => en.ToString();
    }

    public class AuthorizePolicies : AuthorizeAttribute
    {
        public AuthorizePolicies(params Consts.Policy[] policies)
        {
            var allowedPoliciesAsStrings = policies.Select(x => Enum.GetName(typeof(Consts.Policy), x));
            Policy = string.Join(",", allowedPoliciesAsStrings);
        }
    }

    public class Consts
    {
        public const int PageSize = 9;
        public const int AttributeTop = 3;

        public enum Policy
        {
            ManageProducts,
            ManageSocial,
            ManageUser,
            Checkout,
            Auction
        }

        public enum SortType
        {
            Popularity,
            Newness,
            Rating,
            Price
        }

        public enum Status
        {
            Public,
            Hidden,
            Deleted
        }

        public enum OrderStatus
        {
            Processing,
            Delivering,
            Delivered,
            Cancelled
        }
        public const int NoPreference = -1;

        public static readonly Image DefaultImage = new Image
        {
            Url = ""
        };

        public enum ReviewStatus
        {
            Enabled,
            Disabled
        }

        public enum HubActivity
        {
            RefreshCart,
            AddProduct,
            UpdateProduct,
            RemoveProduct
        }
        public enum Toastr
        {
            Success,
            Info,
            Warning,
            Error
        }

        public enum CouponType
        {
            Date,
            Quantity
        }

        public enum CachePrefix
        {
            ProductUrl,
            ProductId,
            CategoryId,
            CategoryUrl,
            CurrentUser
        }

        public static string GetCachePrefix(CachePrefix prefix, object id)
        {
            return prefix + "-" + id;
        }

        public enum PaymentMethod
        {
            CashOnDelivery,
            PayPal
        }
    }
}
