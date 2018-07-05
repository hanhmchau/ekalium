using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

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

        public IdentityRepository(ApplicationDbContext ctx, UserManager<User> userManager,
            SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = ctx;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> GetCurrentUserAsync() => await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
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
