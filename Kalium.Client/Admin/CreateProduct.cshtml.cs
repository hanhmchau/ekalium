using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Client.Admin
{
    public class CreateProductModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected ICollection<Category> Categories { get; set; } = new List<Category>();
        protected ICollection<Brand> Brands { get; set; } = new List<Brand>();
        protected ICollection<string> Origins { get; set; } = new List<string>();
        protected ICollection<string> Materials { get; set; }
        protected string Name { get; set; }
        protected int Brand { get; set; }
        protected int Category { get; set; }
        protected string Price { get; set; }
        protected string DiscountedPrice { get; set; }
        protected bool HasDiscount { get; set; }
        protected string Origin { get; set; }
        protected string Material { get; set; }
        protected string Features { get; set; }
        protected string Description { get; set; }
        protected int Quantity { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "outside");
            await LoadCategories();
            await LoadAttributes();
            RegisteredFunction.Invoke<bool>("enableCreateProductValidation", "outside");
            MegaService.Util.InitializeSignalR();
            MegaService.Util.InitAdminComponents();
        }

        private async Task LoadCategories()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/category/LoadCategories/");
            string productJson = cateJObject["Categories"].ToString();
            Categories = JsonConvert.DeserializeObject<ICollection<Category>>(productJson);
        }

        private async Task LoadAttributes()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/product/LoadBrands/");
            string brandJson = cateJObject["Brands"].ToString();
            Brands = JsonConvert.DeserializeObject<ICollection<Brand>>(brandJson);
        }

        protected async Task Create()
        {
            Brand = int.Parse(MegaService.Util.GetInput<string>("#selectize-brand").Replace("\"",""));
            Category = int.Parse(MegaService.Util.GetInput<string>("#selectize-category").Replace("\"", ""));
            var obj = await MegaService.Fetcher.Fetch("/api/Product/CreateProduct", new
            {
                Brand,
                Category,
                Origin,
                Material,
                Description,
                Features,
                Name,
                Price,
                HasDiscount,
                DiscountedPrice
            });
            var url = obj["Url"].ToString();

            MegaService.Util.RefreshShop();
            MegaService.Util.ShowModal("create-success");

            await Task.Delay(TimeSpan.FromSeconds(3));
            MegaService.UriHelper.NavigateTo($"/admin/product/{url}");
        }
    }
}
