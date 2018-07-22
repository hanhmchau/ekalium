using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kalium.Shared.Models
{
    public enum Role
    {
        Admin,
        Moderator,
        Member
    }
    public static class RoleExtensions
    {
        public static string GetRoleName(this Role role) // convenience method
        {
            return role.ToString();
        }
    }

    public enum EClaim
    {
        ProductManager,
        UserManager,
        SocialManager,
        Checkout,
        Auction
    }
    public static class ClaimExtensions
    {
        public static string GetClaimName(this EClaim eClaim) // convenience method
        {
            return eClaim.ToString();
        }
    }
}
