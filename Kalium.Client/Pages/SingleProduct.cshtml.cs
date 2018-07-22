using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace Kalium.Client.Pages
{
    public class SingleProductModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        [Parameter]
        protected string ProductUrl { get; set; }
        [Parameter]
        protected Product Product { get; set; }
        protected double TotalPrice { get; set; }
        protected Image BigImage { get; set; }
        protected int Quantity { get; set; }
        protected Dictionary<Extra, Option> extraChoices = new Dictionary<Extra, Option>();
        protected int Star { get; set; } = 5;
        protected bool CanReview { get; set; }
        protected string ReviewContent { get; set; }
        protected User CurrentUser { get; set; }
        protected bool IsAuthorized { get; set; }

        //    protected HubConnection Connection { get; set; }

        protected void AddToCart()
        {
            if (extraChoices.Any(entry => !entry.Key.Optional && entry.Value == null))
            {
                DirectToastr(Consts.Toastr.Warning.ToString().ToLower(), "Please select all mandatory options.");
                return;
            }
            Quantity = RegisteredFunction.Invoke<int>("getInput", "#product-quantity");
            AppendCartStorage(Product.Id, Quantity, extraChoices);
            DirectToastr(Consts.Toastr.Success.ToString().ToLower(), "Added to cart.");
        }

        protected async Task AddReview()
        {
            var id = await MegaService.HttpClient.PostJsonAsync<int>("/api/Product/AddReview", JsonConvert.SerializeObject(
                new
                {
                    Content = ReviewContent,
                    Rating = Star,
                    ProductId = Product.Id
                }));

            var review = new Review
            {
                Content = ReviewContent,
                DateCreated = DateTime.Now,
                Id = id,
                User = CurrentUser,
                Rating = Star
            };
            if (Product.Reviews == null)
            {
                Product.Reviews = new List<Review>();
            }
            Product.Reviews.Add(review);
            CanReview = false;
            MegaService.Toastr.Success("Review added successfully.");
            StateHasChanged();
        }

        protected async Task DeleteReview(Review review)
        {
            await MegaService.HttpClient.PostJsonAsync("/api/Product/DeleteReview", JsonConvert.SerializeObject(
                new
                {
                    ReviewId = review.Id
                }));
            StateHasChanged();
            MegaService.Toastr.Success("Review deleted successfully.");
            CanReview = await MegaService.AccountService.CanReview(Product.Id);
            Product.Reviews.Remove(review);
        }

        protected void UpdateBigImage(Image newBig)
        {
            BigImage = newBig;
            StateHasChanged();
        }

        protected void SelectOption(object val, Extra extra)
        {
            var chosenOption = int.Parse(val.ToString());
            extraChoices[extra] = extra.Options.FirstOrDefault(opt => opt.Id == chosenOption);
            TotalPrice = extraChoices.Where(key => key.Value != null).Sum(key => key.Value.Price) + Product.DiscountedPrice;
            StateHasChanged();
        }

        public Option GetOption(Extra extra)
        {
            if (extraChoices.TryGetValue(extra, out var option))
            {
                return option;
            }
            return null;
        }

        protected async Task LoadProduct()
        {
            var jObject = await MegaService.Fetcher.Fetch($"/api/Product/GetProductByUrl?url={ProductUrl}");
            Product = JsonConvert.DeserializeObject<Product>(jObject["Product"].ToString());
            IsAuthorized = await MegaService.AccountService.IsAuthorized(Consts.Policy.ManageProducts);
            CurrentUser = await MegaService.AccountService.GetCurrentUser();
            if (Product == null || Product.Status == (int)Consts.Status.Deleted)
            {
                MegaService.Util.NavigateToNotFound();
                return;
            }

            if (Product.Status == (int)Consts.Status.Hidden && !IsAuthorized)
            {
                MegaService.Util.NavigateToForbidden();
                return;
            }

            TotalPrice = Product.DiscountedPrice;
            BigImage = Product.MainImage;
            Product.Extras.ForEach(extra =>
            {
                extraChoices.Add(extra, null);
            });
            Product.Reviews = Product.Reviews?.OrderByDescending(r => r.DateCreated).ToList();
            StateHasChanged();
            CanReview = await MegaService.AccountService.CanReview(Product.Id);
            StateHasChanged();
        }

        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("initializeSignalR");
            await LoadProduct();
            RegisteredFunction.Invoke<bool>("tabs");
            RegisteredFunction.Invoke<bool>("slick");
        }

        protected void DirectToastr(string mode, string message)
        {
            RegisteredFunction.Invoke<bool>("toastr", mode, message);
        }

        protected T GetStorage<T>(string key)
        {
            var json = RegisteredFunction.Invoke<string>("getStorage", key);
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected void AppendCartStorage(int productId, int quantity, Dictionary<Extra, Option> extraChoices)
        {
            RegisteredFunction.Invoke<bool>("appendCartStorage", new
            {
                guid = Guid.NewGuid().ToString(),
                id = productId,
                quantity,
                extraChoices
            });
        }

        protected void ColorStar(int star)
        {
            Star = star;
            RegisteredFunction.Invoke<bool>("colorStar", star);
        }
    }
}
