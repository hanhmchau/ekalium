using System;
using System.Collections.Generic;
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
    public class ManageProductModel: BlazorComponent
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
        protected string CategoryName { get; set; }
        [Parameter]
        protected Category Category { get; set; }
        [Parameter]
        protected ICollection<Product> Products { get; set; } = new List<Product>();
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
        protected Product ToDelete { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "outside");
            await LoadProducts();
            await LoadCategories();
            MegaService.Util.InitializeSignalR();
            MegaService.Util.InitAdminComponents();
        }

        protected async Task Delete()
        {
            if (ToDelete != null)
            {
                var resultJson = await MegaService.Fetcher.Fetch("/api/Product/Delete", new
                {
                    ToDelete.Id
                });
                var succeeded = resultJson["Result"].ToObject<bool>();
                if (succeeded)
                {
                    MegaService.Toastr.Success("Successfully deleted.");
                    if (Products.Count == 1)
                    {
                        Page--;
                    }
                    await LoadProducts();
                    MegaService.Util.RefreshShop();
                }
                else
                {
                    MegaService.Toastr.Warning("Failed to delete. Please try again.");
                }
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
            await ResetProduct();
        }

        protected async Task ResetProduct()
        {
            Page = 1;
            await LoadProducts();
        }

        protected async Task ChangePage(int p)
        {
            Page = p;
            await LoadProducts();
        }

        protected async Task LoadProducts()
        {
            if (CategoryName != null && CategoryName.Equals("-1"))
            {
                CategoryName = "";
            }
            JObject data = await MegaService.Fetcher.Fetch("/api/Product/LoadProducts/", new
            {
                Phrase,
                PageSize,
                Page,
                CategoryName,
                SortType,
                MinPrice = -1,
                MaxPrice = -1,
                ChosenOrigins = new List<string>(),
                ChosenMaterials = new List<string>(),
                ChosenBrands = new List<Brand>(),
                IncludeHidden = true
            });

            string productJson = data["Products"].ToString();
            var newProducts = JsonConvert.DeserializeObject<ICollection<Product>>(productJson);
            Products.Clear();
            newProducts.ForEach(p =>
            {
                Products.Add(p);
            });
            Total = (int)data["Total"];
            Console.WriteLine(Page);
            TotalPage = (int)Math.Ceiling(Total * 1.0 / PageSize);
            Begin = Math.Max((Page - 1) * PageSize + 1, 1);
            End = Math.Min(Page * PageSize, Total);
            StateHasChanged();
        }

        protected async Task LoadCategories()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/category/LoadCategories/");
            string productJson = cateJObject["Categories"].ToString();
            Categories = JsonConvert.DeserializeObject<ICollection<Category>>(productJson);
        }
    }
}
