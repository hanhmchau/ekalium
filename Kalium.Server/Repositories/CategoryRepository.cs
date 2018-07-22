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
        public CategorySearchHelper Like(string phrase)
        {
            Collection = Collection.Where(c => c.Name.Contains(phrase));
            return this;
        }

        public CategorySearchHelper IncludeCountHidden()
        {
            Collection.ForEach(col =>
                col.ProductCount = Context.Entry(col).Collection(c => c.Products).Query().Count(p => p.Status != (int)Consts.Status.Deleted));
            return this;
        }

        public CategorySearchHelper IncludeCountPublic()
        {
            Collection.ForEach(col =>
                col.ProductCount = Context.Entry(col).Collection(c => c.Products).Query().Count(p => p.Status == (int) Consts.Status.Public));
            return this;
        }

        public CategorySearchHelper IncludeProducts()
        {
            Collection = Collection.Include(cat => cat.Products);
            return this;
        }

        public CategorySearchHelper SortBy(Consts.SortType sortType)
        {
            Expression<Func<Category, IComparable>> comparator = c => c.Name;
            switch (sortType)
            {
                case Consts.SortType.Newness:
                    comparator = c => c.Id;
                    break;
                case Consts.SortType.Popularity:
                    comparator = p => p.ProductCount;
                    break;
            }

            Collection = Collection.OrderByDescending(comparator);
            return this;
        }

        public CategorySearchHelper Active()
        {
            Collection = Collection.Where(cat => !cat.Deleted);
            return this;
        }
    }

    public interface ICategoryRepository
    {
        Task<Category> FindCategoryById(int id);
        Task<Category> FindCategoryByName(string name);
        Task<ICollection<Category>> SearchCategories();
        Task<ICollection<Category>> SearchCategories(string phrase, int sortType, int page, int pageSize);
        Task<int> GetCategoryCount(int id);
        Task DeleteCategory(int id);
        Task Update(Category category);
        Task Create(string name);
        Task<int> CountCategories(string phrase, int sortType);
        Task<ICollection<Category>> SearchTopCategories(int top);
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

        public async Task<int> GetCategoryCount(int id)
        {
            var cat = await _context.Category.FindAsync(id);
            var count = _context.Entry(cat).Collection(c => c.Products).Query()
                .Count(p => p.Status == (int) Consts.Status.Public);
            return count;
        }

        public async Task DeleteCategory(int id)
        {
            var cat = await _context.Category.FindAsync(id);
            cat.Deleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task Update(Category category)
        {
            var cat = await _context.Category.FindAsync(category.Id);
            cat.Name = category.Name;
            await _context.SaveChangesAsync();
        }

        public async Task Create(string name)
        {
            var newCat = new Category
            {
                Name = name,
                Deleted = false
            };
            _context.Category.Add(newCat);
            await _context.SaveChangesAsync();
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
        public async Task<ICollection<Category>> SearchTopCategories(int top)
        {
            var searcher = new CategorySearchHelper(_context);
            var col = await searcher
                .Active()
                .IncludeCountPublic()
                .Get();
            return col.OrderByDescending(c => c.ProductCount).Where(c => c.ProductCount > 0).Take(top).ToList();
        }

        public async Task<ICollection<Category>> SearchCategories()
        {
            var searcher = new CategorySearchHelper(_context);
            return await searcher
                .Active()
                .IncludeCountPublic()
                .SortBy(Consts.SortType.Popularity)
                .Get();
        }

        public async Task<ICollection<Category>> SearchCategories(string phrase, int sortType, int page, int pageSize)
        {
            var searcher = new CategorySearchHelper(_context);
            return await searcher
                .Active()
                .IncludeCountPublic()
                .Like(phrase)
                .SortBy((Consts.SortType) sortType)
                .Page(page, pageSize)
                .Get();
        }

        public async Task<int> CountCategories(string phrase, int sortType)
        {
            var searcher = new CategorySearchHelper(_context);
            return await searcher
                .Active()
                .IncludeCountPublic()
                .Like(phrase)
                .Count();
        }
    }
}
