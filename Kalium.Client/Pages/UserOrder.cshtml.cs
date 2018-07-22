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
    public class UserOrderModel : BlazorLayoutComponent
    {
        protected bool Loaded { get; set; }
        protected ICollection<OrderData> Orders { get; set; }
        [Parameter]
        protected string UserName { get; set; }
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected User User { get; set; }
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

                JObject cateJObject = await MegaService.Fetcher.Fetch("/api/Order/SearchOrders/", new
                {
                    SortType = Consts.SortType.Newness,
                    Page = -1,
                    PageSize = -1,
                    OrderStatus = -1,
                    StartDate = "",
                    EndDate = "",
                    User = User.Id
                });
                string productJson = cateJObject["Orders"].ToString();
                Orders = JsonConvert.DeserializeObject<ICollection<OrderData>>(productJson);
                Loaded = true;
            }
        }
    }
}
