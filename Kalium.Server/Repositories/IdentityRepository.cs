using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Kalium.Server.Repositories
{
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
            _cache.Set(cachePrefix, newUser);
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
            return await _userManager.FindByNameAsync(username);
        }
    }
}
