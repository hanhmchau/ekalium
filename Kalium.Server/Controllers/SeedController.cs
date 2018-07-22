using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Server.HubR;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class SeedController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductHub _productHub;
        private readonly IEmailSender _emailSender;

        public SeedController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, 
            ApplicationDbContext ctx, IHttpContextAccessor httpContextAccessor, IProductHub productHub, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = ctx;
            _httpContextAccessor = httpContextAccessor;
            _productHub = productHub;
            _emailSender = emailSender;
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Product()
        {
            ICollection<Category> categories = new[] { "Notepad", "Calendar", "Pencil", "Postcard", "Notebooks", "Art Print" }
                .Select(name => new Category {Name = name}).ToList();
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var random = new Random();
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Wildflowers",
                    Category = categories.First(cat => cat.Name.Equals("Notepad")),
                    Creator = currentUser,
                    Price = 15,
                    DiscountedPrice = 12,
                    Images = new List<Image>{new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/n/p/npb002-wildflowers-01_1.jpg" } },
                    Featured = false,
                    Material = "Warm White Text Paper",
                    Process = "Full color + Foil stamp",
                    PageCount = 65,
                    Origin = "USA",
                    Quantity = 10,
                    Format = "6 x 9\"",
                    Status = (int) Consts.Status.Public,
                    NameUrl = "wildflowers",
                    ReviewStatus = (int) Consts.ReviewStatus.Enabled,
                    Description = @"Framed up by a ring of gently bending flowers, we left plenty of
                    blank space in the middle of our Wildflower memo pad. “Don’t
                    forget to put your laundry away!” will never look more cheerful.".Replace("\n", ""),
                    DateCreated = DateTime.Now
                },
                new Product
                {
                    Name = "2019 Floral Vines",
                    Category = categories.First(cat => cat.Name.Equals("Calendar")),
                    Creator = currentUser,
                    Price = random.Next(100, 200),
                    DiscountedPrice = random.Next(10, 100),
                    Images = new List<Image>
                    {
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/p/l/plm014-floral-vines-planner-1_1.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/p/l/plm014-floral-vines-planner-2.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/p/l/plm014-floral-vines-planner-3.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/p/l/plm014-floral-vines-planner-4.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/p/l/plm014-floral-vines-planner-5.jpg" }
                    },
                    Featured = false,
                    Material = "Natural White Text Paper",
                    Process = "Full color + Foil stamp",
                    PageCount = random.Next(20, 80),
                    Origin = "France",
                    Quantity = random.Next(1, 100),
                    Format = "6.75 x 8.25\"",
                    Status = (int) Consts.Status.Public,
                    NameUrl = "2019-floral-vines",
                    ReviewStatus = (int) Consts.ReviewStatus.Disabled,
                    Description = @"We created our Classic Planners for the true agenda lover, starting
                                with our playful Floral Vines pattern. The sturdy hardcover
                                wraps over the inside spiral binding, and an elastic band holds
                                the pages closed. This planner features gold foil accents on the
                                cover, monthly and weekly views, inspirational quotes, a ruled
                                pocket in the back, and for the first time ever — sticker sheets!".Replace("\n", ""),
                    Features = @"17-month weekly + monthly calendar pages
                    August 2018 - December 2019
                    Inspirational Quotes
                    Metallic Gold Accents
                    3 Sticker Pages
                    Pocket Folder with Ruler
                    Sections for celebrations, notes, and important contacts".Replace("\n", ""),
                    DateCreated = DateTime.Now
                },
                new Product
                {
                    Name = "Folk",
                    Category = categories.First(cat => cat.Name.Equals("Pencil")),
                    Creator = currentUser,
                    Price = 50,
                    DiscountedPrice = 50,
                    Images = new List<Image>
                    {
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/b/p/bpa001-folk-01_1.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/b/p/bpa001-folk-02.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/b/p/bpa001-folk-03_2.jpg" } ,
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/b/p/bpa001-folk-04-r.jpg" } ,
                    },
                    Featured = true,
                    Material = " Wood + Graphite",
                    Process = "Full color",
                    Origin = "France",
                    Quantity = random.Next(1, 100),
                    Format = "7.5\"",
                    Status = (int) Consts.Status.Public,
                    NameUrl = "folk",
                    ReviewStatus = (int) Consts.ReviewStatus.Enabled,
                    Features = @"3 of each design
                                    Black erasers
                                    Pre-sharpened".Replace("\n", ""),
                    Description = @"Our folk writing pencil set includes a total of 12 pencils, 3
                                of each design. These come pre-sharpened and ready to use.".Replace("\n", ""),
                    DateCreated = DateTime.Now
                },
                new Product
                {
                    Name = "Sending You My Love",
                    Category = categories.First(cat => cat.Name.Equals("Calendar")),
                    Creator = currentUser,
                    Price = random.Next(100, 200),
                    DiscountedPrice = random.Next(10, 100),
                    Images = new List<Image>
                    {
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/s/e/sending-you-my-love-valentines-day-postcard-01_1_2.jpg" } ,
                    },
                    Featured = false,
                    Material = "Natural White Cover Paper",
                    Process = "Full color",
                    PageCount = random.Next(20, 80),
                    Origin = "France",
                    Quantity = random.Next(1, 100),
                    Format = "6.75 x 8.25\"",
                    Status = (int) Consts.Status.Public,
                    NameUrl = "sending-you-my-love",
                    ReviewStatus = (int) Consts.ReviewStatus.Enabled,
                    Features = @"Doubled-sided",
                    Description = @"Let your friends and family know that you’re thinking about them this
Valentine’s Day! These postcards feature a hand-painted illustration 
on the front and space for a message and recipient address on the
back. Requires less postage than a standard card in the United States.".Replace("\n", ""),
                    DateCreated = DateTime.Now
                },
                new Product
                {
                    Name = "Ruby Folk",
                    Category = categories.First(cat => cat.Name.Equals("Art Print")),
                    Creator = currentUser,
                    Price = random.Next(100, 200),
                    DiscountedPrice = random.Next(10, 100),
                    Images = new List<Image>
                    {
                        new Image { Url = "https://m.riflepaperco.com/media/catalog/product/cache/1/thumbnail/1170x1248/fb193eecb19491ee2d70d1d38e002e96/a/p/apm104-ruby-folk-02.jpg" } ,
                    },
                    Featured = false,
                    Material = "Bright White Cover Paper",
                    Process = "Archival Full color",
                    Origin = "France",
                    Quantity = random.Next(1, 100),
                    Format = "6.75 x 8.25\"",
                    Status = (int) Consts.Status.Public,
                    NameUrl = "ruby-folk",
                    ReviewStatus = (int) Consts.ReviewStatus.Enabled,
                    Description = @"Archival print created from an original illustration featuring our Ruby Folk design.",
                    Features = @"Ships in a flat protective sleeve",
                    Extras = new List<Extra>
                    {
                        new Extra
                        {
                            Name = "Format",
                            Optional = false,
                            Options = new List<Option>
                            {
                                new Option
                                {
                                    Name = "8 x 10\"",
                                    Price = 24
                                },
                                new Option
                                {
                                    Name = "11 x 14\"",
                                    Price = 40
                                }
                            }
                        }
                    },
                    DateCreated = DateTime.Now
                }
            };

            await _context.Category.AddRangeAsync(categories);
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
            return Ok("Very seeded...");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Email()
        {
            await _emailSender.SendEmailAsync("asanriku@gmail.com", "Confirm your email",
                "I love u really");
            return Ok("Mail me seeded...");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Index()
        {
            // Get the list of the roles from the enum
            Role[] roles = (Role[])Enum.GetValues(typeof(Role));
            foreach (var r in roles)
            {
                // Create an identity role object out of the enum value
                var identityRole = new IdentityRole
                {
                    Id = r.GetRoleName(),
                    Name = r.GetRoleName()
                };

                // Create the role if it doesn't already exist
                if (!await _roleManager.RoleExistsAsync(roleName: identityRole.Name))
                {
                    var result = await _roleManager.CreateAsync(identityRole);

                    // Return 500 if it fails
                    if (!result.Succeeded)
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            var adminRole = await _roleManager.FindByNameAsync(Role.Admin.GetRoleName());
            var moderatorRole = await _roleManager.FindByNameAsync(Role.Moderator.GetRoleName());
            var memberRole = await _roleManager.FindByNameAsync(Role.Member.GetRoleName());

            await _roleManager.AddClaimAsync(adminRole,
                new Claim(ClaimTypes.Name, EClaim.ProductManager.GetClaimName()));
            await _roleManager.AddClaimAsync(adminRole,
                new Claim(ClaimTypes.Name, EClaim.SocialManager.GetClaimName()));
            await _roleManager.AddClaimAsync(adminRole,
                new Claim(ClaimTypes.Name, EClaim.UserManager.GetClaimName()));

            await _roleManager.AddClaimAsync(moderatorRole,
                new Claim(ClaimTypes.Name, EClaim.SocialManager.GetClaimName()));

            await _roleManager.AddClaimAsync(memberRole,
                new Claim(ClaimTypes.Name, EClaim.Auction.GetClaimName()));
            await _roleManager.AddClaimAsync(memberRole,
                new Claim(ClaimTypes.Name, EClaim.UserManager.GetClaimName()));

            // Our default user
            User user = new User
            {
                Email = "fish@gmail.com",
                UserName = "fish",
                LockoutEnabled = false
            };

            // Add the user to the database if it doesn't already exist
            if (await _userManager.FindByEmailAsync(user.Email) == null)
            {
                // WARNING: Do NOT check in credentials of any kind into source control
                // 5ESTdYB5cyYwA2dKhJqyjPYnKUc & 45Ydw ^ gz ^ jy & FCV3gxpmDPdaDmxpMkhpp & 9TRadU % wQ2TUge!TsYXsh77Qmauan3PEG8!6EP
                var result = await _userManager.CreateAsync(user, password: "1234");

                if (!result.Succeeded) // Return 500 if it fails
                    return StatusCode(StatusCodes.Status500InternalServerError);

                // Assign all roles to the default user
                result = await _userManager.AddToRolesAsync(user, roles.Select(r => r.GetRoleName()));

                if (!result.Succeeded) // Return 500 if it fails
                    return StatusCode(StatusCodes.Status500InternalServerError);

                await _signInManager.PasswordSignInAsync("fish", "1234", true, false);
            }

            // All good, 200 OK!            
            return Ok("Seeded...");
        }

        [HttpGet("[action]")]
        public async Task<string> Password()
        {
            var user = await _userManager.FindByNameAsync("Luculia");
            var hash = _userManager.PasswordHasher.HashPassword(user, "1234");
            user.PasswordHash = hash;
            await _context.SaveChangesAsync();
            return "Omega lul";
        }

        [HttpGet("[action]")]
        public async Task<string> Announce()
        {
            await _productHub.SendMessage(@"Red", @"Don’t you hate it when someone answers their own questions? I do!");
            return "Lulz";
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Brand()
        {
            var brandNames = new[] {"Staedtler", "Faber-Castell", "Maped", "Schwan-Stabilo", "Muji", "Artline"};
            var brands = brandNames.Select(name => new Brand
            {
                Name = name
            }).ToList();
            await _context.Brand.AddRangeAsync(brands);
            await _context.SaveChangesAsync();
            return Ok("Famously seeded...");
        }
    }
}
