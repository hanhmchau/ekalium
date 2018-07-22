using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoreLinq;

namespace Kalium.Server.Repositories
{

    internal class CategorySearchHelper : SearchHelper<Category>
    {
        public CategorySearchHelper(ApplicationDbContext context) : base(context)
        {
            Collection = context.Category;
        }
        public CategorySearchHelper IncludeCount()
        {
            Collection.ForEach(col =>
                col.ProductCount = Context.Entry(col).Collection(c => c.Products).Query().Count());
            return this;
        }

        public CategorySearchHelper IncludeProducts()
        {
            Collection = Collection.Include(cat => cat.Products);
            return this;
        }

        public CategorySearchHelper SortByPopularity()
        {
            Collection = Collection.OrderByDescending(cat => cat.ProductCount);
            return this;
        }
        public CategorySearchHelper Active()
        {
            Collection = Collection.Where(cat => cat.Status);
            return this;
        }
    }

    public interface ICategoryRepository
    {
        Task<Category> FindCategoryById(int id);
        Task<Category> FindCategoryByName(string name);
        Task<ICollection<Category>> SearchCategories();
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public CategoryRepository(ApplicationDbContext ctx, IMemoryCache cache)
        {
            _context = ctx;
            _cache = cache;
        }

        public async Task<Category> FindCategoryById(int id)
        {
            var cacheKey = Consts.GetCachePrefix(Consts.CachePrefix.CategoryId, id);
            if (_cache.TryGetValue(cacheKey, out var cat))
            {
                return cat as Category;
            }

            var newCat = await _context.Category.FindAsync(id);
            _cache.Set(cacheKey, newCat);
            return newCat;
        }
        public async Task<Category> FindCategoryByName(string name)
        {
            var cacheKey = Consts.GetCachePrefix(Consts.CachePrefix.CategoryUrl, name);
            if (_cache.TryGetValue(cacheKey, out var c))
            {
                return c as Category;
            }

            var newProduct = await _context.Category.FirstOrDefaultAsync(cat => cat.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            _cache.Set(cacheKey, newProduct);
            return newProduct;
        }
        public async Task<ICollection<Category>> SearchCategories()
        {
            var searcher = new CategorySearchHelper(_context);
            return await searcher
                .Active()
                .IncludeCount()
                .SortByPopularity()
                .Get();
        }
    }
}
