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
using Microsoft.AspNetCore.Authorization;
using MoreLinq;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class CartController : Controller
    {
        private readonly IProductRepository _iProductRepository;
        private readonly ICheckoutRepository _iCheckoutRepository;

        public CartController(IProductRepository productRepository, ICheckoutRepository iCheckoutRepository)
        {
            this._iProductRepository = productRepository;
            _iCheckoutRepository = iCheckoutRepository;
        }

        private async Task<ECart> FromPseudo(PseudoCart pseudoCart)
        {
            var cart = new ECart();
            foreach (var item in pseudoCart.Cart)
            {
                var product = await _iProductRepository.FindProductByIdForCart(item.Id);
                if (product != null && product.Status == (int) Consts.Status.Public)
                {
                    var cartItem = new ECartItem
                    {
                        Product = product,
                        Quantity = item.Quantity,
                        Guid = item.Guid
                    };

                    item.Choices.ForEach(choice =>
                    {
                        var extra = cartItem.Product.Extras.FirstOrDefault(ext => ext.Id == choice.Extra);
                        var option = extra?.Options.FirstOrDefault(opt => opt.Id == choice.Option.GetValueOrDefault(-1));
                        if (extra != null && option != null)
                        {
                            cartItem.Choices.Add(extra, option);
                        }
                    });

                    cart.Contents.Add(cartItem);
                }
            }

            cart.Coupons = cart.Contents.SelectMany(item => item.Product.Coupons)
                .Where(c => pseudoCart.Coupons.Contains(c.Id)).Distinct().ToList();

            return cart;
        }

        [HttpPost("[action]")]
        public async Task<string> Get([FromBody] string json)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.Message);
                throw;
            }
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.Checkout)]
        public async Task<string> CheckOut([FromBody] string json)
        {
            var parser = new Parser(json);
            var pseudoCart = parser.AsObject<PseudoCart>("PseudoCart");
            var name = parser.AsString("Name");
            var address = parser.AsString("Address");
            var phone = parser.AsString("Phone");
            var alternateName = parser.AsString("AlternateName");
            var alternateAddress = parser.AsString("AlternateAddress");
            var alternatePhone = parser.AsString("AlternatePhone");
            var paymentMethod = parser.AsInt("PaymentMethod");
            var sendToDifferentAddress = parser.AsBool("SendToDifferentAddress");
            var note = parser.AsString("Note");

//            var eCart = await FromPseudo(pseudoCart);

            var checkOutResult = await _iCheckoutRepository.CheckOut(pseudoCart, name, address, phone, sendToDifferentAddress, alternateName,
                alternateAddress, alternatePhone, paymentMethod, note);
            object result = new
            {
                Result = checkOutResult
            };
            
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
