using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
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
            var isAuthorized = await MegaService.AccountService.IsAuthorized(Consts.Policy.ManageProducts);
            if (Product == null || Product.Status == (int)Consts.Status.Deleted)
            {
                MegaService.Util.NavigateToNotFound();
                return;
            }

            if (Product.Status == (int)Consts.Status.Hidden && !isAuthorized)
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
    }
}
