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
            var user = MegaService.AccountService.GetCurrentUser();
            if (user == null)
            {
                MegaService.UriHelper.NavigateTo("/login");
                return;
            }

            if (int.TryParse(OrderId, out var numberId))
            {
                var orderJson = await MegaService.Fetcher.Fetch($"/api/Order/FindOrder?id={numberId}");
                var code = int.Parse(orderJson["Code"].ToString());
                if (code == 200)
                {
                    Order = orderJson["Order"].ToObject<OrderData>();
                }
                else
                {
                    MegaService.UriHelper.NavigateTo($"/{code}");
                }
            }
        }

        protected async Task CancelOrder()
        {
            Console.WriteLine("Helloooooo, Hellooooo?");
            var result = await MegaService.Fetcher.Fetch($"/api/Order/Cancel?id={Order.Id}");
            var code = result["Code"].ToObject<int>();
            if (code == 200)
            {
                var refundDate = result["RefundDate"].ToObject<DateTime>();
                var refundRate = result["RefundRate"].ToObject<double>();
                Order.RefundDate = refundDate;
                MegaService.Toastr.Success("Order cancelled.");
                MegaService.Util.HideModal();
            }
            else
            {
                MegaService.Toastr.Error("Cannot cancel order.");
            }
        }
    }
}
