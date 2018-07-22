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
    public class UserPasswordModel : BlazorLayoutComponent
    {
        protected bool Loaded { get; set; }
        protected bool DuplicateUsername { get; set; }
        protected bool DuplicateEmail { get; set; }
        [Parameter]
        protected string UserName { get; set; }
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected User User { get; set; }
        protected string OldPassword { get; set; }
        protected string NewPassword { get; set; }
        protected string ConfirmPassword { get; set; }
        protected User CurrentUser { get; set; }
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
            }
        }

        protected async Task Save()
        {
            var validated = MegaService.Util.ValidateForm("#shipping-form");
            if (validated)
            {
                var result = await MegaService.HttpClient.PostJsonAsync<bool>("/api/Identity/UpdatePassword", JsonConvert.SerializeObject(new 
                {
                    User.Id,
                    OldPassword,
                    NewPassword
                }));

                if (result)
                {
                    MegaService.Toastr.Success("Successfully updated.");
                }
                else
                {
                    MegaService.Toastr.Warning("Incorrect current message.");
                }

            }
            StateHasChanged();
        }
    }
}
