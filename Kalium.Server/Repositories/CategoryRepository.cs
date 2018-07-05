using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace Kalium.Server.Repositories
{

    internal class CategorySearchHelper : SearchHelper<Category>
    {
        public CategorySearchHelper(IQueryable<Category> collection, ApplicationDbContext context) : base(collection, context)
        {
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

        public CategoryRepository(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public async Task<Category> FindCategoryById(int id)
        {
            return await _context.Category.FindAsync(id);
        }
        public async Task<Category> FindCategoryByName(string name)
        {
            return await _context.Category.FirstOrDefaultAsync(cat => cat.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        public async Task<ICollection<Category>> SearchCategories()
        {
            var searcher = new CategorySearchHelper(_context.Category, _context);
            return await searcher
                .Active()
                .IncludeCount()
                .SortByPopularity()
                .Get();
        }
    }
}
