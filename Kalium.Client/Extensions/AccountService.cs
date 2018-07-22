using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Kalium.Shared.Services;
using Microsoft.AspNetCore.Blazor;

namespace Kalium.Client.Extensions
{
    public interface IAccountService
    {
        Task<User> GetCurrentUser();
        Task<bool> IsAuthorized(Consts.Policy policy);
    }

    public class AccountService : IAccountService
    {
        private readonly HttpClient _http;
        private readonly IFetcher _fetcher;

        public AccountService(HttpClient http, IFetcher fetcher)
        {
            _http = http;
            _fetcher = fetcher;
        }

        public async Task<User> GetCurrentUser()
        {
            var user = await _http.GetJsonAsync<User>("/api/Identity/GetCurrentUser/");
            return user;
        }

        public async Task<bool> IsAuthorized(Consts.Policy policy)
        {
            return await _http.GetJsonAsync<bool>($"/api/Identity/IsUserAuthorized?policy={(int) policy}");
        }
    }
}
