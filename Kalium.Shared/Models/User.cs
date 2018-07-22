using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kalium.Shared.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        [NotMapped] public string RealAvatar => Avatar ?? Consts.Consts.DefaultAvatar;
        [NotMapped] public string Role { get; set; }
        public DateTime DateRegistered { get; set; }
        [NotMapped]
        public ICollection<string> Roles { get; set; }
        [NotMapped]
        public string MainRole => RolePriority.HighestRole(Roles);
        public ICollection<Order> Orders { get; set; }
        [NotMapped]
        public double TotalPaid => Orders?.Sum(o => o.PostCouponTotal) ?? 0;
        [NotMapped]
        public double TotalPaidBackup { get; set; }
        public bool SubscribedToEmail { get; set; }
    }
}
