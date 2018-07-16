using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;

namespace Kalium.Client.Admin
{
    public class DashboardModel: BlazorComponent
    {
        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("initDashboard");
        }
    }
}
