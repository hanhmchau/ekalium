using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Kalium.Shared.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
    }
}
