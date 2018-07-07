using System;
using System.Collections.Generic;
using System.Text;
using Kalium.Shared.Models;

namespace Kalium.Shared.Consts
{
    public class Consts
    {
        public const int PageSize = 9;
        public const int AttributeTop = 3;

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
            CategoryUrl
        }

        public static string GetCachePrefix(CachePrefix prefix, object id)
        {
            return prefix + "-" + id;
        }
    }
}
