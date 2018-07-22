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

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            this._categoryRepository = categoryRepository;
        }

        [HttpGet("[action]")]
        public async Task<string> LoadCategories()
        {
            object result = new
            {
                Categories = await _categoryRepository.SearchCategories(),
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> SearchCategories([FromBody] string json)
        {
            var parser = new Parser(json);
            var phrase = parser.AsString("Phrase");
            var sortType = parser.AsInt("SortType");
            var page = parser.AsInt("Page");
            var pageSize = parser.AsInt("PageSize");
            object result = new
            {
                Categories = await _categoryRepository.SearchCategories(phrase, sortType, page, pageSize),
                Total = await _categoryRepository.CountCategories(phrase, sortType)
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

            var count = await _categoryRepository.GetCategoryCount(id);
            if (count == 0)
            {
                await _categoryRepository.DeleteCategory(id);
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
            Category category = parser.AsObject<Category>("Category");

            await _categoryRepository.Update(category);

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
            string name = parser.AsString("CategoryName");

            await _categoryRepository.Create(name);

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
