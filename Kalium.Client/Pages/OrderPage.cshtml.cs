using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Front;
using Kalium.Shared.Models;
using Kalium.Shared.Services;
using Microsoft.AspNetCore.Blazor.Components;
using Newtonsoft.Json;

namespace Kalium.Client.Pages
{
    public class OrderModel: BlazorComponent
    {
        [Parameter]
        protected string OrderId { get; set; }
        protected OrderData Order { get; set; }
        [Inject]
        public IMegaService MegaService { get; set; }

        protected override async Task OnInitAsync()
        {
            User user = null;
            var authorized = false;
            try
            {
                user = await MegaService.AccountService.GetCurrentUser();
                authorized = await MegaService.AccountService.IsAuthorized(Consts.Policy.ManageProducts);
                if (user == null)
                {
                    MegaService.Util.Checkpoint($"/order/{OrderId}");
                    MegaService.UriHelper.NavigateTo("/login");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (int.TryParse(OrderId, out var numberId))
            {
                var orderJson = await MegaService.Fetcher.Fetch($"/api/Order/FindOrder?id={numberId}");
                var code = int.Parse(orderJson["Code"].ToString());
                if (code == 200)
                {
                    Order = orderJson["Order"].ToObject<OrderData>();
                    if (user != null && !Order.UserId.Equals(user.Id) && !authorized)
                    {
                        MegaService.Util.NavigateToForbidden();
                    }
                }
                else
                {
                    MegaService.UriHelper.NavigateTo($"/{code}");
                }
            }
        }

        protected async Task CancelOrder()
        {
            var result = await MegaService.Fetcher.Fetch($"/api/Order/Cancel?id={Order.Id}");
            var code = result["Code"].ToObject<int>();
            if (code == 200)
            {
                var refundDate = result["RefundDate"].ToObject<DateTime>();
                var refundRate = result["RefundRate"].ToObject<double>();
                Order.RefundDate = refundDate;
                Order.Status = (int) Consts.OrderStatus.Cancelled;
                MegaService.Toastr.Success("Order cancelled.");
                MegaService.Util.HideModal();
                StateHasChanged();
            }
            else
            {
                MegaService.Toastr.Error("Cannot cancel order.");
            }
        }
    }
}
