using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace Kalium.Client.AdminWide
{
    public class UpdateProductModel : BlazorComponent
    {
        [Parameter]
        protected string ProductUrl { get; set; }
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected Product Product { get; set; }
        protected ICollection<Category> Categories { get; set; } = new List<Category>();
        protected ICollection<Brand> Brands{ get; set; } = new List<Brand>();
        protected string Name { get; set; }
        protected bool IsPublic { get; set; }
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
        protected Coupon NewCoupon { get; set; }
        protected Extra NewExtra { get; set; }
        protected bool HasNewExtra { get; set; }
        protected string CouponDateStr { get; set; }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>
                ("removeCss", "outside");
            await LoadCategories();
            await LoadAttributes();
            RegisteredFunction.Invoke<bool>
                ("enableCreateProductValidation", "outside");
            MegaService.Util.InitializeSignalR();
            var productJson = await MegaService.Fetcher.Fetch($"/api/Product/GetProductByUrlForAdmin?url={ProductUrl}");
            Product = JsonConvert.DeserializeObject<Product>(productJson["Product"].ToString());
            if (Product != null)
            {
                StateHasChanged();
                MegaService.Util.InitAdminComponents();
                IsPublic = Product.Status == (int) Consts.Status.Public;
            }
            else
            {
                MegaService.Util.NavigateToNotFound();
            }
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

        protected async Task DeleteImage(Image image)
        {
            Product.Images.Remove(image);
            // TODO: SAVE STUFFIE TO SERVER HERE
            var saveResult = await MegaService.Fetcher.Fetch("/api/product/RemoveImage/", new
            {
                Product.Id,
                Image = image
            });
            var saveBool = saveResult["Result"].ToObject<bool>();
            Toastr(saveBool);
            StateHasChanged();
        }

        private void Toastr(bool result)
        {
            if (result)
            {
                MegaService.Toastr.Success("Updated.");
            }
            else
            {
                MegaService.Toastr.Error("Update failed. Please try again.");
            }
            AnnounceUpdate();
        }

        private void AnnounceUpdate()
        {
            MegaService.Util.AnnounceUpdateProduct(Product.Id);
        }

        protected async Task UpdateProduct()
        {
            if (HasDiscount)
            {
                double.TryParse(DiscountedPrice, out var newPrice);
                Product.DiscountedPrice = newPrice;
            }
            else
            {
                Product.DiscountedPrice = Product.Price;
            }
            Product.Category = Categories.FirstOrDefault(cat => cat.Id == Category);
            Product.Brand = Brands.FirstOrDefault(brand => brand.Id == Brand);
            Product.Status = (int) (IsPublic ? Consts.Status.Public : Consts.Status.Hidden);
            var result = await MegaService.HttpClient.PostJsonAsync<bool>("/api/product/Update/", Product);
            Toastr(result);
            StateHasChanged();
        }

        protected async Task Create()
        {
            var json = JsonConvert.SerializeObject(Product);
        }

        protected async Task SaveExtra(Extra extra)
        {
            var result = RegisteredFunction.Invoke<bool>("validateExtra", $"#extra_optional_{extra.Id}");
            if (result)
            {
                // TODO: SAVE STUFFIE TO SERVER HERE
                var saveResult = await MegaService.Fetcher.Fetch("/api/product/UpdateExtra/", new
                {
                    Product.Id,
                    Extra = extra
                });
                var saveBool = saveResult["Result"].ToObject<bool>();
                var newExtra = saveResult["Extra"].ToObject<Extra>();
                extra.Id = newExtra.Id;
                extra.Options.Clear();
                newExtra.Options.ForEach(o =>
                {
                    extra.Options.Add(o);
                });
                Toastr(saveBool);
                StateHasChanged();
            }
        }

        protected void AddExtra()
        {
            if (Product.Extras.All(e => e.Id != 0))
            {
                Product.Extras.Add(new Extra
                {
                    Options = new List<Option>()
                });
            }
        }

        protected async Task RemoveCoupons(Coupon coupon)
        {
            coupon.Deleted = true;
            // TODO: SAVE STUFFIE TO SERVER HERE
            var saveResult = await MegaService.Fetcher.Fetch("/api/product/RemoveCoupon/", new
            {
                Product.Id,
                Coupon = NewCoupon
            });
            var saveBool = saveResult["Result"].ToObject<bool>();
            Toastr(saveBool);
            StateHasChanged();
        }

        protected async Task RemoveExtra(Extra extra)
        {
            extra.Deleted = true;
            // TODO: SAVE STUFFIE TO SERVER HERE
            var saveResult = await MegaService.Fetcher.Fetch("/api/product/RemoveExtra/", new
            {
                Product.Id,
                Extra = extra.Id
            });
            var saveBool = saveResult["Result"].ToObject<bool>();
            Toastr(saveBool);
            StateHasChanged();
        }

        protected async Task AddNewCoupon()
        {
            var result = await RegisteredFunction.InvokeAsync<bool>("validateCoupon");
            if (result)
            {
                if (NewCoupon.Type == (int) Consts.CouponType.Date)
                {
                    CouponDateStr = MegaService.Util.GetInput<string>("#new-coupon-date");
                    var parseResult = DateTime.TryParse(CouponDateStr, out var dateTime);
                    if (!parseResult)
                    {
                        return;
                    }
                    NewCoupon.DateExpired = dateTime;
                }
                NewCoupon.Id = -1; //signify the coupon as not saved yet
                Product.Coupons.Add(NewCoupon);

                // TODO: SAVE STUFFIE TO SERVER HERE
                var saveResult = await MegaService.Fetcher.Fetch("/api/product/AddCoupon/", new
                {
                    Product.Id,
                    Coupon = NewCoupon
                });
                var saveBool = saveResult["Result"].ToObject<bool>();

                Toastr(saveBool);
                NewCoupon = null;
                StateHasChanged();
            }
        }

        protected async Task Email()
        {
            JObject cateJObject = await MegaService.Fetcher.Fetch("/api/Product/EmailToSubscribers", new
            {
                ProductId = Product.Id
            });
            var count = cateJObject["Count"].ToObject<int>();
            if (count > -1)
            {
                MegaService.Toastr.Success($"Email successfully sent to {count} user{(count > 1 ? "s" : "")}");
            }
            else
            {
                MegaService.Toastr.Warning("Could not send email. Please check your internet connection.");
            }
        }
    }
}
