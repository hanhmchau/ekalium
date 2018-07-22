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

    public static class RolePriority
    {
        private static readonly IDictionary<string, int> Dict = new Dictionary<string, int>
        {
            {Role.Admin.ToString(), 0},
            {Role.Moderator.ToString(), 1},
            {Role.Member.ToString(), 2}
        };

        public static int Priority(string role)
        {
            return Dict[role];
        }

        public static string HighestRole(ICollection<string> roles)
        {
            return roles?.OrderBy(Priority).First();
        }

        public static ICollection<string> HigherRoles(string role)
        {
            return Dict.Keys.Where(key => Dict[key] < Dict[role]).ToList();
        }
        public static ICollection<string> LowerRoles(string role)
        {
            return Dict.Keys.Where(key => Dict[key] >= Dict[role]).ToList();
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
