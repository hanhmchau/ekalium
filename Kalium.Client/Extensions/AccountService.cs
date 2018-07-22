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
using Newtonsoft.Json;

namespace Kalium.Client.Extensions
{

    public interface IAccountService
    {
        Task<User> GetCurrentUser();
        Task<bool> IsAuthorized(Consts.Policy policy);
        Task<bool> IsDuplicateUsername(string username);
        Task<bool> IsDuplicateEmail(string email);
        Task<bool> CanReview(int productId);
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


        public async Task<bool> IsDuplicateUsername(string username)
        {
            return await _http.PostJsonAsync<bool>("/api/Identity/CheckDuplicateUsername/", JsonConvert.SerializeObject(new
            {
                Username = username
            }));
        }

        public async Task<bool> IsDuplicateEmail(string email)
        {
            return await _http.PostJsonAsync<bool>("/api/Identity/CheckDuplicateEmail/", JsonConvert.SerializeObject(new
            {
                Email = email
            }));
        }

        public async Task<bool> CanReview(int productId)
        {
            return await _http.PostJsonAsync<bool>("/api/Product/CanReview/", JsonConvert.SerializeObject(new
            {
                ProductId = productId
            }));
        }
    }
}
