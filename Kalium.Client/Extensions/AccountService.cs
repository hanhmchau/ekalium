using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor;

namespace Kalium.Client.Extensions
{
    public interface IAccountService
    {
        Task<User> GetCurrentUser();
    }

    public class AccountService : IAccountService
    {
        private readonly HttpClient _http;

        public AccountService(HttpClient http)
        {
            _http = http;
        }

        public async Task<User> GetCurrentUser()
        {
            var user = await _http.GetJsonAsync<User>("/api/Identity/GetCurrentUser/");
            return user;
        }
    }
}
