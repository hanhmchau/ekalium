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
using Kalium.Shared;
using Kalium.Shared.Consts;
using Microsoft.AspNetCore.Authorization;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class IdentityController : Controller
    {
        private readonly IIdentityRepository _identityRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IImageRepository _imageRepository;

        public IdentityController(IIdentityRepository identityRepository, IAuthorizationService authorizationService, IImageRepository imageRepository)
        {
            this._identityRepository = identityRepository;
            _authorizationService = authorizationService;
            _imageRepository = imageRepository;
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

        [HttpPost("[action]")]
        public async Task<ICollection<Image>> UploadAvatar([FromForm] AvatarViewModel model)
        {
            ICollection<Image> images = new List<Image>();
            var files = model.Files;
            if (files != null && files.Any())
            {
                images = _imageRepository.Save(files, Consts.Folder.User.Name());
                await _identityRepository.AddImages(model.Id, images.First());
            }

            return images;
        }

        [HttpPost("[action]")]
        public async Task<bool> UpdateUser([FromBody] string json)
        {
            var parser = new Parser(json);
            string id = parser.AsString("Id");
            string email = parser.AsString("Email");
            string address = parser.AsString("Address");
            string phone = parser.AsString("Phone");
            string fullName = parser.AsString("FullName");
            await _identityRepository.UpdateUser(id, email, address, phone, fullName);

            return true;
        }

        [HttpGet("[action]")]
        public async Task<bool> IsUserAuthorized([FromQuery] int policy)
        {
            if (HttpContext.User == null)
            {
                return false;
            }

            var policyName = ((Consts.Policy) policy).Name();
            var result = await _authorizationService.AuthorizeAsync(HttpContext.User, null, policyName);
            return result.Succeeded;
        }

        [HttpGet("[action]")]
        public async Task<User> GetUser([FromQuery] string username)
        {
            return await _identityRepository.FindUserByUsername(username);
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

        [HttpPost("[action]")]
        public async Task<bool> UpdatePassword([FromBody] string json)
        {
            var parser = new Parser(json);
            string id = parser.AsString("Id");
            string oldPassword = parser.AsString("OldPassword");
            string newPassword = parser.AsString("NewPassword");
            var result = await _identityRepository.UpdatePassword(id, oldPassword, newPassword);
            return result;
        }

        [HttpPost("[action]")]
        public async Task<bool> UpdateRole([FromBody] string json)
        {
            var parser = new Parser(json);
            string id = parser.AsString("Id");
            string role = parser.AsString("Role");
            var result = await _identityRepository.UpdateRole(id, role);
            return result;
        }

        [HttpPost("[action]")]
        public async Task<string> SearchUsers([FromBody] string json)
        {
            var parser = new Parser(json);
            var phrase = parser.AsString("Phrase");
            var role = parser.AsString("Role");
            var page = parser.AsInt("Page");
            var pageSize = parser.AsInt("PageSize");
            var sortType = parser.AsInt("SortType");
            var result = new
            {
                Users = await _identityRepository.SearchUsers(phrase, role, sortType, page, pageSize),
                Total = await _identityRepository.CountUsers(phrase, role)
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
