using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Kalium.Shared.Models;
using Kalium.Shared.Services;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Client.Pages
{
    public class CartModel : BlazorComponent
    {
//        protected ECart ECart { get; set; }
//        protected bool Loaded { get; set; } = false;
//        [Inject]
//        protected LocalStorage Storage { get; set; }
//        [Inject]
//        protected IFetcher Fetcher { get; set; }
//        [Inject]
//        protected HttpClient Http { get; set; }
//        public async void RefreshCart()
//        {
//            var cartJson = Storage["CART"];
//            if (cartJson != null)
//            {
//                var pseudoCart = JsonConvert.DeserializeObject<PseudoCart>(cartJson, new JsonSerializerSettings
//                {
//                    NullValueHandling = NullValueHandling.Include
//                });
//                if (pseudoCart.Cart.Any())
//                {
//                    var ecartJson = await Fetcher.Fetch("/api/Cart/Get/", pseudoCart);
//                    ECart = ecartJson["ECart"].ToObject<ECart>();
//                }
//            }
//        }
    }
}
