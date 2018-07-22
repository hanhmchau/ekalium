using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Front;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Client.Admin
{
    public class ManageOrderModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected ICollection<OrderData> Orders { get; set; }
        [Parameter]
        protected int SortType { get; set; } = (int) Consts.SortType.Newness;
        protected int Page { get; set; } = 1;
        protected int PageSize { get; set; } = 10;
        protected int TotalPage { get; set; }
        protected int Total { get; set; }
        protected int Begin { get; set; }
        protected int End { get; set; }
        protected bool Loaded { get; set; } = false;
        protected string SortTypeStr { get; set; } = "Newest first";
        protected Category ToDelete { get; set; }
        protected string NewCategory { get; set; }
        protected int OrderStatus { get; set; } = -1;
        protected string StartDate { get; set; }
        protected string EndDate { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "outside");
            await LoadOrders();
//            MegaService.Util.InitializeSignalR();
            MegaService.Util.InitAdminComponents();
            Loaded = true;
        }

        protected async Task Save(OrderData orderData, int status)
        {
            var resultJson = await MegaService.Fetcher.Fetch("/api/Order/UpdateStatus", new
            {
                orderData.Id,
                Status = status,
            });
            var succeeded = resultJson["Result"].ToObject<bool>();
            if (succeeded)
            {
                MegaService.Toastr.Success("Successfully updated.");
                orderData.Status = status;
            }
            else
            {
                MegaService.Toastr.Warning("Update failed. Please try again");
            }
        }
        
        protected async Task ChangeSortType(Consts.SortType sortType)
        {
            SortType = (int)sortType;
            switch (sortType)
            {
                case Consts.SortType.Newness:
                    SortTypeStr = "Newest first";
                    break;
                case Consts.SortType.Price:
                    SortTypeStr = "Highest total first";
                    break;
            }
            await LoadOrders();
        }

        protected async Task UpdateDateRange()
        {
            var dateRange = MegaService.Util.GetDates("#date-range-picker");
            StartDate = dateRange.Begin;
            EndDate = dateRange.End;
            await LoadOrders();
        }

        protected async Task FilterStatus(int orderStatus)
        {
            OrderStatus = orderStatus;
            await LoadOrders();
        }

        protected async Task ResetOrders()
        {
            Page = 1;
            await LoadOrders();
        }

        protected async Task ChangePage(int p)
        {
            Page = p;
            await LoadOrders();
        }
        
        protected async Task LoadOrders()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/Order/SearchOrders/", new
            {
                SortType,
                Page,
                PageSize,
                OrderStatus,
                StartDate,
                EndDate,
                User = ""
            });
            string productJson = cateJObject["Orders"].ToString();
            Orders = JsonConvert.DeserializeObject<ICollection<OrderData>>(productJson);
            Total = (int )cateJObject["Total"];
            TotalPage = (int)Math.Ceiling(Total * 1.0 / PageSize);
            Begin = Math.Max((Page - 1) * PageSize + 1, 1);
            End = Math.Min(Page * PageSize, Total);
            StateHasChanged();
        }
    }
}
