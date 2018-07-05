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

        public class ProductHubActivity
        {
            public const string Add = "ADD";
            public const string Update = "UPDATE";
            public const string Delete = "Delete";
        }
    }
}
