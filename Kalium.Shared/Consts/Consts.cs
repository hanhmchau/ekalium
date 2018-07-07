using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kalium.Shared.Consts
{
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
    }
}
