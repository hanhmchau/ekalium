using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Client.Admin
{
    public class ManageUserModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        [Parameter]
        protected string Phrase { get; set; } = "";
        [Parameter]
        protected int Page { get; set; } = 1;
        [Parameter]
        protected int PageSize { get; set; } = 10;
        [Parameter]
        protected Category Category { get; set; }
        [Parameter]
        protected ICollection<User> Users { get; set; } = new List<User>();
        [Parameter]
        protected int SortType { get; set; } = (int) Consts.SortType.Newness;

        protected string Role { get; set; } = "";
        protected int TotalPage { get; set; }
        protected int Total { get; set; }
        protected int Begin { get; set; }
        protected int End { get; set; }
        protected bool Loaded { get; set; } = false;
        protected string SortTypeStr { get; set; } = "Newest first";
        protected Category ToDelete { get; set; }
        protected string NewCategory { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "outside");
            await LoadUsers();
//            MegaService.Util.InitializeSignalR();
            MegaService.Util.InitAdminComponents();
            Loaded = true;
        }

        protected async Task Delete()
        {
            if (ToDelete != null)
            {
                var resultJson = await MegaService.Fetcher.Fetch("/api/Category/Delete", new
                {
                    ToDelete.Id
                });
                var succeeded = resultJson["Result"].ToObject<bool>();
                if (succeeded)
                {
                    MegaService.Toastr.Success("Successfully deleted.");
                }
            }
        }

        protected async Task Save(User user, string role)
        {
            var succeeded = await MegaService.HttpClient.PostJsonAsync<bool>("/api/Identity/UpdateRole", JsonConvert.SerializeObject(new
            {
                user.Id,
                Role = role
            }));
            if (succeeded)
            {
                MegaService.Toastr.Success("Successfully updated.");
                var lowerRoles = RolePriority.LowerRoles(role);
                user.Roles = lowerRoles;
            }
            else
            {
                MegaService.Toastr.Warning("Update failed. Please try again");
            }
            StateHasChanged();
        }
        
        protected async Task SearchPhrase(string phrase)
        {
            Phrase = phrase;
            await LoadUsers();
        }

        protected async Task ChangeSortType(Consts.SortType sortType)
        {
            SortType = (int)sortType;
            switch (sortType)
            {
                case Consts.SortType.Newness:
                    SortTypeStr = "Newest first";
                    break;
                case Consts.SortType.Popularity:
                    SortTypeStr = "Oldest first";
                    break;
                case Consts.SortType.Rating:
                    SortTypeStr = "Highest rating first";
                    break;
                case Consts.SortType.Price:
                    SortTypeStr = "Cheapest first";
                    break;
                case Consts.SortType.Alphabet:
                    SortTypeStr = "Alphabet order";
                    break;
            }
            await LoadUsers();
        }

        protected async Task ResetCategories()
        {
            Page = 1;
            await LoadUsers();
        }

        protected async Task ChangePage(int p)
        {
            Page = p;
            await LoadUsers();
        }
        
        protected async Task LoadUsers()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/Identity/SearchUsers/", new
            {
                Phrase,
                Role,
                Page,
                PageSize,
                SortType
            });
            string productJson = cateJObject["Users"].ToString();
            Users = JsonConvert.DeserializeObject<ICollection<User>>(productJson);
            Total = (int )cateJObject["Total"];
            TotalPage = (int)Math.Ceiling(Total * 1.0 / PageSize);
            Begin = Math.Max((Page - 1) * PageSize + 1, 1);
            End = Math.Min(Page * PageSize, Total);
            StateHasChanged();
        }
    }
}
