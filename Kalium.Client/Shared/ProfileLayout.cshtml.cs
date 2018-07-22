using System;
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
    public class ProfileLayoutModel: BlazorLayoutComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected bool Loaded { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "metronic");
            MegaService.Util.InitComponents();
            //            MegaService.Util.InitAdminComponents();
            Loaded = true;
        }
    }
}
