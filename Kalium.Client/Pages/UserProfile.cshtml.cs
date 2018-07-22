using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Front;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Client.Pages
{
    public class UserProfileModel : BlazorLayoutComponent
    {
        protected bool Loaded { get; set; }
        protected bool DuplicateUsername { get; set; }
        protected bool DuplicateEmail { get; set; }
        [Parameter]
        protected string UserName { get; set; }
        protected User CurrentUser { get; set; }
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected User User { get; set; }
        protected string FullName { get; set; }
        protected string Address { get; set; }
        protected string Phone { get; set; }
        protected string Email { get; set; }
        protected override async Task OnInitAsync()
        {
            MegaService.Util.InitComponents();
            CurrentUser = await MegaService.AccountService.GetCurrentUser();
            MegaService.Util.Checkpoint($"/user/{UserName}");
            if (CurrentUser == null)
            {
                MegaService.UriHelper.NavigateTo("/login");
                return;
            }
            User = await MegaService.HttpClient.GetJsonAsync<User>($"/api/Identity/GetUser?username={UserName}");
            StateHasChanged();
            if (User != null)
            {
                if (!User.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase))
                {
                    MegaService.UriHelper.NavigateTo("/403");
                    return;
                }
                var isAuthorized = await MegaService.AccountService.IsAuthorized(Consts.Policy.ManageUser);
                if (!isAuthorized)
                {
                    MegaService.UriHelper.NavigateTo("/403");
                    return;
                }
                Loaded = true;
                FullName = User.FullName;
                Address = User.Address;
                Phone = User.PhoneNumber;
            }
        }

        protected async Task ChangeUsername(string username)
        {
            DuplicateUsername = await MegaService.AccountService.IsDuplicateUsername(username) ||
                                username.Equals(User.UserName);
        }

        protected void ChangeImage()
        {
            var image = MegaService.Util.GetInput<string>("#hidden-avatar");
            Console.WriteLine($"Hey: {image}");
            User.Avatar = image;
        }

        protected async Task ChangeEmail(string email)
        {
            DuplicateEmail = await MegaService.AccountService.IsDuplicateEmail(email) && !email.Equals(User.Email);
        }

        protected async Task Save()
        {
            ChangeImage();
            var validated = MegaService.Util.ValidateForm("#shipping-form");
            Email = MegaService.Util.GetInput<string>("#account_email");
            DuplicateEmail = await MegaService.AccountService.IsDuplicateEmail(Email) && !Email.Equals(User.Email);
            if (validated && !DuplicateEmail)
            {
                var result = await MegaService.HttpClient.PostJsonAsync<bool>("/api/Identity/UpdateUser", JsonConvert.SerializeObject(new 
                {
                    Id = User.Id,
                    Email,
                    Phone,
                    Address,
                    FullName
                }));

                User.Email = Email;
                User.PhoneNumber = Phone;
                User.Address = Address;
                User.FullName = FullName;
                MegaService.Toastr.Success("Successfully updated.");
            }
            StateHasChanged();
        }
    }
}
