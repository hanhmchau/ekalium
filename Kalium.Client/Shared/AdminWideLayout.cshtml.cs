﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;

namespace Kalium.Client.Shared
{
    public class AdminWideLayoutModel: BlazorLayoutComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected bool Loaded { get; set; }
        protected User CurrentUser { get; set; }

        protected override async Task OnInitAsync()
        {
            CurrentUser = await MegaService.AccountService.GetCurrentUser();
            MegaService.Util.Checkpoint("/admin");
            if (CurrentUser == null)
            {
                MegaService.UriHelper.NavigateTo("/login");
                return;
            }
            var isAuthorized = await MegaService.AccountService.IsAuthorized(Consts.Policy.ManageProducts);
            if (!isAuthorized)
            {
                MegaService.UriHelper.NavigateTo("/403");
                return;
            }
            RegisteredFunction.Invoke<bool>("removeCss", "outside");
//            MegaService.Util.InitAdminComponents();
            Console.WriteLine("I load you you load me");
            Loaded = true;
        }
    }
}
