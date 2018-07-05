using Kalium.Server.Context;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public SampleDataController(ApplicationDbContext ctx, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = ctx;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("[action]")]
        public IEnumerable<User> GetUsers() => _context.Users.ToList();

        [HttpGet("[action]")]
        public string GetName() => "Planetarian";
    }
}
