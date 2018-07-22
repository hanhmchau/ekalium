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

    internal class BrandSearchHelper : SearchHelper<Brand>
    {
        public BrandSearchHelper AsNoTracking()
        {
            Collection = Collection.AsNoTracking();
            return this;
        }
        public BrandSearchHelper(ApplicationDbContext context) : base(context)
        {
            Collection = context.Brand;
        }
        public BrandSearchHelper Like(string phrase)
        {
            Collection = Collection.Where(c => c.Name.Contains(phrase));
            return this;
        }

        public BrandSearchHelper IncludeCountHidden()
        {
            Collection.ForEach(col =>
                col.ProductCount = Context.Entry(col).Collection(c => c.Products).Query().Count(p => p.Status != (int)Consts.Status.Deleted));
            return this;
        }

        public BrandSearchHelper IncludeCountPublic()
        {
            Collection.ForEach(col =>
                col.ProductCount = Context.Entry(col).Collection(c => c.Products).Query().Count(p => p.Status == (int) Consts.Status.Public));
            return this;
        }

        public BrandSearchHelper IncludeProducts()
        {
            Collection = Collection.Include(cat => cat.Products)
                .ThenInclude(p => p.OrderItems)
                .ThenInclude(o => o.Order);
//                        .ThenInclude(o => o.Coupons);
            return this;
        }

        public BrandSearchHelper SortBy(Consts.SortType sortType)
        {
            Expression<Func<Brand, IComparable>> comparator = c => c.Name;
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

        public BrandSearchHelper Active()
        {
            Collection = Collection.Where(cat => !cat.Deleted);
            return this;
        }
    }

    public interface IBrandRepository
    {
        Task<Brand> FindBrandById(int id);
        Task<int> GetBrandCount(int id);
        Task DeleteBrand(int id);
        Task Update(Brand brand);
        Task Create(string name);
        Task<Brand> FindBrandByName(string name);
        Task<ICollection<Brand>> SearchBrands();
        Task<ICollection<Brand>> SearchBrands(string phrase, int sortType, int page, int pageSize);
        Task<int> CountBrands(string phrase, int sortType);
        Task<ICollection<Brand>> SearchTopBrands(int i);
    }

    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public BrandRepository(ApplicationDbContext ctx, IMemoryCache cache)
        {
            _context = ctx;
            _cache = cache;
        }

        public async Task<Brand> FindBrandById(int id)
        {
            var newCat = await _context.Brand.FindAsync(id);
            return newCat;
        }

        public async Task<int> GetBrandCount(int id)
        {
            var cat = await _context.Brand.FindAsync(id);
            var count = _context.Entry(cat).Collection(c => c.Products).Query()
                .Count(p => p.Status == (int) Consts.Status.Public);
            return count;
        }

        public async Task DeleteBrand(int id)
        {
            var cat = await _context.Brand.FindAsync(id);
            cat.Deleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task Update(Brand brand)
        {
            var cat = await _context.Brand.FindAsync(brand.Id);
            cat.Name = brand.Name;
            await _context.SaveChangesAsync();
        }

        public async Task Create(string name)
        {
            var newCat = new Brand
            {
                Name = name,
                Deleted = false
            };
            _context.Brand.Add(newCat);
            await _context.SaveChangesAsync();
        }

        public async Task<Brand> FindBrandByName(string name)
        {
            var newProduct = await _context.Brand.FirstOrDefaultAsync(cat => cat.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return newProduct;
        }
        public async Task<ICollection<Brand>> SearchBrands()
        {
            var searcher = new BrandSearchHelper(_context);
            return await searcher
                .Active()
                .IncludeCountPublic()
                .SortBy(Consts.SortType.Popularity)
                .Get();
        }
        public async Task<ICollection<Brand>> SearchTopBrands(int top)
        {
            var searcher = new BrandSearchHelper(_context);
            var col = await searcher
                .Active()
                .IncludeCountPublic()
                .IncludeProducts()
                .AsNoTracking()
                .Get();
            col.ForEach(b =>
            {
                b.AverageRatingBackup = b.AverageRating;
                b.QuantitySoldBackup = b.QuantitySold;
                b.TotalEarningBackup = b.TotalEarning;
                b.ProductCount = b.Products.Count;
            });
            return col.OrderByDescending(c => c.TotalEarningBackup).Where(c => c.TotalEarningBackup > 0).Take(top).ToList();
        }

        public async Task<ICollection<Brand>> SearchBrands(string phrase, int sortType, int page, int pageSize)
        {
            var searcher = new BrandSearchHelper(_context);
            var col = await searcher
                .Active()
                .IncludeCountPublic()
                .Like(phrase)
                .SortBy((Consts.SortType) sortType)
                .IncludeProducts()
                .AsNoTracking()
                .Page(page, pageSize)
                .Get();
            col.ForEach(b =>
            {
                b.AverageRatingBackup = b.AverageRating;
                b.QuantitySoldBackup = b.QuantitySold;
                b.TotalEarningBackup = b.TotalEarning;
                b.ProductCount = b.Products.Count;
            });
            return col;
        }

        public async Task<int> CountBrands(string phrase, int sortType)
        {
            var searcher = new BrandSearchHelper(_context);
            return await searcher
                .Active()
                .IncludeCountPublic()
                .Like(phrase)
                .Count();
        }
    }
}
