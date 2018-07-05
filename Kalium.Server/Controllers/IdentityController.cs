using System;
using Kalium.Server.Context;
using Kalium.Server.Utils;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Kalium.Server.Repositories;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class IdentityController : Controller
    {
        private readonly IIdentityRepository _identityRepository;

        public IdentityController(IIdentityRepository identityRepository)
        {
            this._identityRepository = identityRepository;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TryRegister([FromBody] string json)
        {
            var parser = new Parser(json);
            var user = new User
            {
                Email = parser.AsString("Email"),
                UserName = parser.AsString("Username")
            };
            string password = parser.AsString("Password");
            var result = await _identityRepository.CreateUser(user, password);
            if (result.Succeeded)
            {
                await _identityRepository.AddToRole(user, Role.Member.GetRoleName());
                await _identityRepository.SignIn(user.UserName, password, true, false);
                return Json(user);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CheckDuplicateEmail([FromBody] string json)
        {
            var parser = new Parser(json);
            string email = parser.AsString("Email");
            bool result = await _identityRepository.IsEmailUsed(email);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CheckDuplicateUsername([FromBody] string json)
        {
            var parser = new Parser(json);
            string username = parser.AsString("Username");
            bool result = await _identityRepository.IsUsernameUsed(username);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<bool> IsLoggedIn()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _identityRepository.GetCurrentUserAsync();
            return Json(user);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> LogOut()
        {
             _identityRepository.SignOut();
            return Ok(true);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TryLogin([FromBody] string json)
        {
            var parser = new Parser(json);
            string username = parser.AsString("Username");
            string password = parser.AsString("Password");
            bool rememberMe = parser.AsBool("RememberMe");
            var result = await _identityRepository.SignIn(username, password, rememberMe, true);
            if (result.Succeeded)
            {
                var user = await _identityRepository.FindUserByUsername(username);
                return Json(user);
            }
            else
            {
                return Json(null);
            }
        }
    }
}
