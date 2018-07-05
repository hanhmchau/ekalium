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

        [HttpPost("[action]")]
        public async Task<string> AddToCart([FromBody] string json)
        {
            var parser = new Parser(json);
            int productId = parser.AsInt("ProductId");
            int quantity = parser.AsInt("Quantity");
            object result = new
            {
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> LoadAttributes()
        {
            int top = Consts.AttributeTop;

            var origins = await _iProductRepository.GetOrigins(top);
            var materials = await _iProductRepository.GetMaterials(top);
            object result = new
            {
                Origins = origins,
                Materials = materials
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<Product> GetProductByUrl([FromQuery] string url)
        {
            var product = await _iProductRepository.FindProductByUrl(url);
            product.Category.Products.Clear();
            product.Extras.ForEach(ext => { ext.Product = null; ext.Options.ForEach(opt => opt.Extra = null); });
            return product;
        }
    }
}
