using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoreLinq;

namespace Kalium.Server.Repositories
{

    internal class IdentitySearchHelper : SearchHelper<User>
    {
        private readonly UserManager<User> _userManager;

        public IdentitySearchHelper AsNoTracking()
        {
            Collection = Collection.AsNoTracking();
            return this;
        }
        public IdentitySearchHelper(ApplicationDbContext context, UserManager<User> userManager) : base(context)
        {
            _userManager = userManager;
            Collection = context.Users;
        }
        public IdentitySearchHelper Subscribed()
        {
            Collection = Collection.Where(c => c.SubscribedToEmail);
            return this;
        }

        public IdentitySearchHelper Like(string phrase)
        {
            if (!string.IsNullOrWhiteSpace(phrase))
            {
                Collection = Collection.Where(c => c.UserName.Contains(phrase) || c.Email.Contains(phrase));
            }
            return this;
        }

        public IdentitySearchHelper IncludeOrders()
        {
            Collection = Collection
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(u => u.Orders).ThenInclude(o => o.Refund)
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderCoupons);
            return this;
        }

        public IdentitySearchHelper SortBy(Consts.SortType sortType)
        {
            Expression<Func<User, IComparable>> comparator = null;
            bool descending = true;
            switch (sortType)
            {
                case Consts.SortType.Newness:
                    comparator = c => c.DateRegistered;
                    break;
                case Consts.SortType.Alphabet:
                    comparator = p => p.UserName;
                    descending = false;
                    break;
                case Consts.SortType.Price:
                    comparator = u => u.TotalPaid;
                    break;
            }

            if (comparator != null)
            {
                Collection = descending ? Collection.OrderByDescending(comparator) : Collection.OrderBy(comparator);
            }
            return this;
        }
    }

    public interface IIdentityRepository
    {
        Task<User> GetCurrentUserAsync();
        Task<bool> IsUsernameUsed(string username);
        Task<bool> IsEmailUsed(string email);
        Task<bool> IsCurrentUserInRole(string role);
        Task<IdentityResult> CreateUser(User user, string password);
        Task<SignInResult> SignIn(string username, string password, bool isPersistent, bool isLockedOut);
        void SignOut();
        Task<IdentityResult> AddToRole(User user, string role);
        Task<User> FindUserByUsername(string username);
        Task AddImages(string userId, Image avatar);
        Task UpdateUser(string id, string email, string address, string phone, string fullName);
        Task<bool> UpdatePassword(string id, string oldPassword, string newPassword);
        Task<ICollection<User>> SearchTopBoughtUsers(int top);
        Task<ICollection<User>> SearchUsers(string phrase, string role, int sortType, int page, int pageSize);
        Task<int> CountUsers(string phrase, string role);
        Task<bool> UpdateRole(string id, string role);
        Task<ICollection<User>> GetSubscribedUsers();
    }

    public class IdentityRepository: IIdentityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public IdentityRepository(ApplicationDbContext ctx, UserManager<User> userManager,
            SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _context = ctx;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var cachePrefix = Consts.GetCachePrefix(Consts.CachePrefix.CurrentUser, 0);
            if (_cache.TryGetValue(cachePrefix, out var user))
            {
                return user as User;
            }
            var newUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (newUser != null)
            {
                newUser.Roles = await _userManager.GetRolesAsync(newUser);
                _cache.Set(cachePrefix, newUser, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                });
            }
            return newUser;
        }
        public async Task<bool> IsUsernameUsed(string username) => await _userManager.FindByNameAsync(username) != null;
        public async Task<bool> IsEmailUsed(string email) => await _userManager.FindByEmailAsync(email) != null;

        public async Task<bool> IsCurrentUserInRole(string role)
        {
            var user = await GetCurrentUserAsync();
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateUser(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<SignInResult> SignIn(string username, string password, bool isPersistent, bool isLockedOut)
        {
            return await _signInManager.PasswordSignInAsync(username, password, isPersistent, isLockedOut);
        }
        public async void SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> AddToRole(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }
        public async Task<User> FindUserByUsername(string username)
        {
            var possibleUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));

            if (possibleUser != null)
            {
                possibleUser.Roles = await _userManager.GetRolesAsync(possibleUser);
            }

            return possibleUser;
        }

        public async Task AddImages(string userId, Image avatar)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.Avatar = avatar.Url;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(string id, string email, string address, string phone, string fullName)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.Email = email;
            user.Address = address;
            user.PhoneNumber = phone;
            user.FullName = fullName;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePassword(string id, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<ICollection<User>> SearchTopBoughtUsers(int top)
        {
            var helper = new IdentitySearchHelper(_context, _userManager);
            var list = await helper
                .IncludeOrders()
                .Get();
            return list.OrderByDescending(u => u.TotalPaid).Where(u => u.TotalPaid > 0).Take(top).ToList();
        }
        public async Task<ICollection<User>> GetSubscribedUsers()
        {
            var helper = new IdentitySearchHelper(_context, _userManager);
            return await helper
                .Subscribed()
                .Get();
        }

        public async Task<ICollection<User>> SearchUsers(string phrase, string role, int sortType, int page, int pageSize)
        {
            var helper = new IdentitySearchHelper(_context, _userManager);
            var col = await helper
                .Like(phrase)
                .SortBy((Consts.SortType) sortType)
                .Get();
            if (!string.IsNullOrWhiteSpace(role))
            {
                ICollection<User> newCol = new List<User>();
                col.ForEach(async u =>
                {
                    if (await _userManager.IsInRoleAsync(u, role))
                    {
                        newCol.Add(u);
                    }
                });
                col = newCol;
            }

            foreach (var u in col)
            {
                u.Roles = await _userManager.GetRolesAsync(u);
            }

            if (sortType == (int) Consts.SortType.Role)
            {
                col = col.OrderBy(u => RolePriority.Priority(u.MainRole)).ToList();
            }

            if (page > -1 && pageSize > -1)
            {
                col = col.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return col;
        }
        public async Task<int> CountUsers(string phrase, string role)
        {
            var helper = new IdentitySearchHelper(_context, _userManager);
            var col = await helper
                .Like(phrase)
                .Get();
            if (!string.IsNullOrWhiteSpace(role))
            {
                ICollection<User> newCol = new List<User>();
                col.ForEach(async u =>
                {
                    if (await _userManager.IsInRoleAsync(u, role))
                    {
                        newCol.Add(u);
                    }
                });
                col = newCol;
            }
            return col.Count;
        }

        public async Task<bool> UpdateRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            var higherRoles = RolePriority.HigherRoles(role);
            var lowerRoles = RolePriority.LowerRoles(role);
            await _userManager.AddToRolesAsync(user, lowerRoles);
            await _userManager.RemoveFromRolesAsync(user, higherRoles);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

