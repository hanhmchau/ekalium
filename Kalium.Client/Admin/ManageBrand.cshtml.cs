using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Client.Admin
{
    public class ManageBrandModel : BlazorComponent
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
        protected ICollection<Brand> Brands { get; set; } = new List<Brand>();
        [Parameter]
        protected ICollection<string> ChosenOrigins { get; set; } = new List<string>();
        [Parameter]
        protected ICollection<string> ChosenMaterials { get; set; } = new List<string>();
        [Parameter]
        protected int SortType { get; set; } = (int)Consts.SortType.Newness;
        protected int TotalPage { get; set; }
        protected int Total { get; set; }
        protected int Begin { get; set; }
        protected int End { get; set; }
        protected bool Loaded { get; set; } = false;
        protected string SortTypeStr { get; set; } = "Newest first";
        protected Brand ToDelete { get; set; }
        protected string NewBrand { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "outside");
            await LoadBrands();
            //            MegaService.Util.InitializeSignalR();
            MegaService.Util.InitAdminComponents();
            Loaded = true;
        }

        protected async Task Delete()
        {
            if (ToDelete != null)
            {
                var resultJson = await MegaService.Fetcher.Fetch("/api/Brand/Delete", new
                {
                    ToDelete.Id
                });
                var succeeded = resultJson["Result"].ToObject<bool>();
                if (succeeded)
                {
                    MegaService.Toastr.Success("Successfully deleted.");
                    if (Brands.Count == 1)
                    {
                        Page = Math.Max(Page--, 1);
                    }
                    await LoadBrands();
                }
                else
                {
                    var count = resultJson["Count"].ToObject<int>();
                    MegaService.Toastr.Warning($"There are {count} products with this brand. Cannot delete.");
                }
            }
        }

        protected async Task Save(Brand brand)
        {
            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                MegaService.Toastr.Warning("Brand name cannot be blank");
                return;
            }

            var resultJson = await MegaService.Fetcher.Fetch("/api/Brand/Update", new
            {
                Brand = brand
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
            if (!MegaService.Util.ValidateForm("#create-brand-form"))
            {
                return;
            }

            var resultJson = await MegaService.Fetcher.Fetch("/api/Brand/Create", new
            {
                BrandName = NewBrand
            });
            var succeeded = resultJson["Result"].ToObject<bool>();
            if (succeeded)
            {
                MegaService.Util.HideModal();
                MegaService.Toastr.Success("Successfully created.");
                if (Brands.Count == PageSize)
                {
                    Page++;
                }
                await LoadBrands();
            }
            else
            {
                MegaService.Toastr.Warning("Update failed. Please try again");
            }
        }

        protected async Task SearchPhrase(string phrase)
        {
            Phrase = phrase;
            await LoadBrands();
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
            await LoadBrands();
        }

        protected async Task ResetBrands()
        {
            Page = 1;
            await LoadBrands();
        }

        protected async Task ChangePage(int p)
        {
            Page = p;
            await LoadBrands();
        }

        protected async Task LoadBrands()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/Brand/SearchBrands/", new
            {
                Phrase,
                SortType,
                Page,
                PageSize
            });
            string productJson = cateJObject["Brands"].ToString();
            Brands = JsonConvert.DeserializeObject<ICollection<Brand>>(productJson);
            Total = (int)cateJObject["Total"];
            TotalPage = (int)Math.Ceiling(Total * 1.0 / PageSize);
            Begin = Math.Max((Page - 1) * PageSize + 1, 1);
            End = Math.Min(Page * PageSize, Total);
            StateHasChanged();
        }
    }
}
