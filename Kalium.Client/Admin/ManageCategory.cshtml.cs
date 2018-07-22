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
    public class ManageCategoryModel: BlazorComponent
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
        protected ICollection<Category> Categories { get; set; } = new List<Category>();
        [Parameter]
        protected ICollection<string> ChosenOrigins { get; set; } = new List<string>();
        [Parameter]
        protected ICollection<string> ChosenMaterials { get; set; } = new List<string>();
        [Parameter]
        protected int SortType { get; set; } = (int) Consts.SortType.Newness;
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
            await LoadCategories();
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
                    if (Categories.Count == 1)
                    {
                        Page = Math.Max(Page--, 1);
                    }
                    await LoadCategories();
                }
                else
                {
                    var count = resultJson["Count"].ToObject<int>();
                    MegaService.Toastr.Warning($"There are {count} products with this category. Cannot delete.");
                }
            }
        }

        protected async Task Save(Category cat)
        {
            if (string.IsNullOrWhiteSpace(cat.Name))
            {
                MegaService.Toastr.Warning("Category name cannot be blank");
                return;
            }

            var resultJson = await MegaService.Fetcher.Fetch("/api/Category/Update", new
            {
                Category = cat
            });
            var succeeded = resultJson["Result"].ToObject<bool>();
            if (succeeded)
            {
                MegaService.Toastr.Success("Successfully updated.");
            }
            else
            {
                MegaService.Toastr.Warning("Update failed. Please try again");
            }
        }

        protected async Task Create()
        {
            if (!MegaService.Util.ValidateForm("#create-category-form"))
            {
                return;
            }

            var resultJson = await MegaService.Fetcher.Fetch("/api/Category/Create", new
            {
                CategoryName = NewCategory
            });
            var succeeded = resultJson["Result"].ToObject<bool>();
            if (succeeded)
            {
                MegaService.Util.HideModal();
                MegaService.Toastr.Success("Successfully created.");
                if (Categories.Count == PageSize)
                {
                    Page++;
                }
                await LoadCategories();
            }
            else
            {
                MegaService.Toastr.Warning("Update failed. Please try again");
            }
        }

        protected async Task SearchPhrase(string phrase)
        {
            Phrase = phrase;
            await LoadCategories();
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
                    SortTypeStr = "Most popular first";
                    break;
                case Consts.SortType.Rating:
                    SortTypeStr = "Highest rating first";
                    break;
                case Consts.SortType.Price:
                    SortTypeStr = "Cheapest first";
                    break;
            }
            await LoadCategories();
        }

        protected async Task ResetCategories()
        {
            Page = 1;
            await LoadCategories();
        }

        protected async Task ChangePage(int p)
        {
            Page = p;
            await LoadCategories();
        }
        
        protected async Task LoadCategories()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/Category/SearchCategories/", new
            {
                Phrase,
                SortType,
                Page,
                PageSize
            });
            string productJson = cateJObject["Categories"].ToString();
            Categories = JsonConvert.DeserializeObject<ICollection<Category>>(productJson);
            Total = (int )cateJObject["Total"];
            TotalPage = (int)Math.Ceiling(Total * 1.0 / PageSize);
            Begin = Math.Max((Page - 1) * PageSize + 1, 1);
            End = Math.Min(Page * PageSize, Total);
            StateHasChanged();
        }
    }
}
