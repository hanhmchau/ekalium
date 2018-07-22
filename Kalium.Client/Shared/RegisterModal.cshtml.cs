using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Newtonsoft.Json;

namespace Kalium.Client.Shared
{
    public class RegisterModalModel: BlazorComponent
    {
        [Parameter]
        protected bool DuplicateEmail { get; set; }
        [Parameter]
        protected bool DuplicateUsername { get; set; }
        [Parameter]
        protected bool PasswordMode { get; set; } = true;
        [Parameter]
        protected string Username { get; set; }
        [Parameter]
        protected string Email { get; set; }
        [Parameter]
        protected string Password { get; set; }
        [Parameter]
        protected User CurrentUser { get; set; }
        [Parameter]
        protected bool HasTypedUsername { get; set; }
        [Parameter]
        protected bool HasTypedEmail { get; set; }
        [Parameter]
        protected bool HasTypedPassword { get; set; }
        [Parameter]
        protected bool GeneralError { get; set; }
        [Parameter]
        protected Action<User> OnUpdateCurrentUser { get; set; }
        [Inject]
        protected IMegaService MegaService { get; set; }

        protected bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Email) && ValidatorUtils.IsValidEmail(Email)
                && !DuplicateEmail && !DuplicateUsername;
        }

        protected void TogglePasswordMode()
        {
            PasswordMode = !PasswordMode;
        }

        protected async void TryRegister()
        {
            if (!IsValid()) return;
            DuplicateEmail = await IsDuplicateEmail(Email);
            DuplicateUsername = await IsDuplicateUsername(Username);
            StateHasChanged();
            if (DuplicateEmail || DuplicateUsername)
            {
                return;
            }
            var result = await MegaService.HttpClient.PostJsonAsync<User>("/api/Identity/TryRegister/", Serialize(new
            {
                Username,
                Email,
                Password
            }));
            if (result != null)
            {
                GeneralError = false;
                CurrentUser = result as User;
                OnUpdateCurrentUser.Invoke(CurrentUser);
                HideModal();
            }
            else
            {
                GeneralError = true;
            }
        }

        protected async void CheckUsername(string username)
        {
            HasTypedUsername = true;
            Username = username;
            DuplicateUsername = await IsDuplicateUsername(Username);
            StateHasChanged();
        }

        protected async Task<bool> IsDuplicateUsername(string username)
        {
            return await MegaService.HttpClient.PostJsonAsync<bool>("/api/Identity/CheckDuplicateUsername/", Serialize(new
            {
                Username
            }));
        }

        protected async void CheckEmail(string email)
        {
            HasTypedEmail = true;
            Email = email;
            DuplicateEmail = await IsDuplicateEmail(Email);
            StateHasChanged();
        }

        protected async Task<bool> IsDuplicateEmail(string email)
        {
            return await MegaService.HttpClient.PostJsonAsync<bool>("/api/Identity/CheckDuplicateEmail/", Serialize(new
            {
                Email
            }));
        }

        protected string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        protected void ShowModal(string id)
        {
            HideModal();
            RegisteredFunction.Invoke<bool>("showModal", id);
        }

        protected void HideModal()
        {
            RegisteredFunction.Invoke<bool>("hideModal");
        }
    }
}
