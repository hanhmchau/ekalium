using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;

namespace Kalium.Client.Shared
{
    public class AdminLayoutModel: BlazorLayoutComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected bool Loaded { get; set; }

        protected override async Task OnInitAsync()
        {
            var user = await MegaService.AccountService.GetCurrentUser();
            MegaService.Util.Checkpoint("/admin");
            if (user == null)
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
            Loaded = true;
        }
    }
}
