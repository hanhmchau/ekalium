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
    public class BrandController : Controller
    {
        private readonly IBrandRepository _brandRepository;

        public BrandController(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [HttpGet("[action]")]
        public async Task<string> LoadBrands()
        {
            object result = new
            {
                Brands = await _brandRepository.SearchBrands(),
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> SearchBrands([FromBody] string json)
        {
            var parser = new Parser(json);
            var phrase = parser.AsString("Phrase");
            var sortType = parser.AsInt("SortType");
            var page = parser.AsInt("Page");
            var pageSize = parser.AsInt("PageSize");
            var col = await _brandRepository.SearchBrands(phrase, sortType, page, pageSize);
            col.ForEach(b => { b.Products = null; });
            object result = new
            {
                Brands = col,
                Total = await _brandRepository.CountBrands(phrase, sortType)
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> Delete([FromBody] string json)
        {
            var parser = new Parser(json);
            int id = parser.AsInt("Id");

            var count = await _brandRepository.GetBrandCount(id);
            if (count == 0)
            {
                await _brandRepository.DeleteBrand(id);
            }

            var result = new
            {
                Result = count == 0,
                Count = count
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> Update([FromBody] string json)
        {
            var parser = new Parser(json);
            Brand brand = parser.AsObject<Brand>("Brand");

            await _brandRepository.Update(brand);

            var result = new
            {
                Result = true
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> Create([FromBody] string json)
        {
            var parser = new Parser(json);
            string name = parser.AsString("BrandName");

            await _brandRepository.Create(name);

            var result = new
            {
                Result = true
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
