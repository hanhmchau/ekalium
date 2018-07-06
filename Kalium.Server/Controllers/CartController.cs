using System;
using Kalium.Server.Context;
using Kalium.Server.Utils;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Kalium.Server.Repositories;
using Kalium.Shared.Consts;
using MoreLinq;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class CartController : Controller
    {
        private readonly IProductRepository _iProductRepository;

        public CartController(IProductRepository productRepository)
        {
            this._iProductRepository = productRepository;
        }

        private async Task<ECart> FromPseudo(PseudoCart pseudoCart)
        {
            var cart = new ECart();
            foreach (var item in pseudoCart.Cart)
            {
                var cartItem = new ECartItem
                {
                    Product = await _iProductRepository.FindProductByIdForCart(item.Id),
                    Quantity = item.Quantity,
                    Guid = item.Guid
                };

                item.Choices.ForEach(choice =>
                {
                    var extra = cartItem.Product.Extras.First(ext => ext.Id == choice.Extra);
                    var option = extra.Options.FirstOrDefault(opt => opt.Id == choice.Option.GetValueOrDefault(-1));
                    cartItem.Choices.Add(extra, option);
                });

                cart.Contents.Add(cartItem);
            }

            cart.Coupons = cart.Contents.SelectMany(item => item.Product.Coupons)
                .Where(c => pseudoCart.Coupons.Contains(c.Id)).Distinct().ToList();

            return cart;
        }

        [HttpPost("[action]")]
        public async Task<string> Get([FromBody] string json)
        {
            var pseudoCart = JsonConvert.DeserializeObject<PseudoCart>(json, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });

            var realCart = await FromPseudo(pseudoCart);

            object result = new
            {
                ECart = realCart
            };

            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
