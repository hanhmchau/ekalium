using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoreLinq;

namespace Kalium.Server.Repositories
{
    internal class ProductSearchHelper : SearchHelper<Product>
    {
        public ProductSearchHelper(ApplicationDbContext context) : base(context)
        {
            Collection = context.Products;
        }
        public ProductSearchHelper IncludeBrands()
        {
            Collection = Collection.Include(p => p.Brand);
            return this;
        }
        public ProductSearchHelper IncludeImages()
        {
            Collection = Collection.Include(p => p.Images);
            return this;
        }
        public ProductSearchHelper IncludeReviews()
        {
            Collection = Collection.Include(p => p.Reviews);
            return this;
        }
        public ProductSearchHelper IncludeAuctions()
        {
            Collection = Collection.Include(p => p.Auctions);
            return this;
        }
        public ProductSearchHelper IncludeDiscussions()
        {
            Collection = Collection.Include(p => p.Discussions);
            return this;
        }
        public ProductSearchHelper IncludeCategory()
        {
            Collection = Collection.Include(p => p.Category);
            return this;
        }
        public ProductSearchHelper IncludeOrderItems()
        {
            Collection = Collection
                .Include(p => p.OrderItems)
                    .ThenInclude(o => o.Order)
                        .ThenInclude(o => o.Refund)
                .Include(p => p.OrderItems)
                    .ThenInclude(o => o.Product)
                .Include(p => p.OrderItems)
                    .ThenInclude(o => o.Order)
                        .ThenInclude(o => o.OrderCoupons);
            return this;
        }
        public ProductSearchHelper IncludeExtras()
        {
            Collection = Collection
                .Include(p => p.Extras)
                .ThenInclude(extra => extra.Options);
            return this;
        }
        public ProductSearchHelper IncludeCoupons()
        {
            Collection = Collection
                .Include(p => p.Coupons);
            return this;
        }
        public ProductSearchHelper HasOrigin(ICollection<string> origins)
        {
            if (origins.Any())
            {
                Collection = Collection.Where(pro => origins.Contains(pro.Origin.Trim()));
            }
            return this;
        }
        public ProductSearchHelper HasMaterial(ICollection<string> materials)
        {
            if (materials.Any())
            {
                Collection = Collection.Where(pro => materials.Contains(pro.Material));
            }
            return this;
        }
        public ProductSearchHelper SortBy(Consts.SortType sortType)
        {
            Expression<Func<Product, IComparable>> comparator = p => p.Name;
            switch (sortType)
            {
                case Consts.SortType.Newness:
                    comparator = p => p.DateCreated;
                    break;
                case Consts.SortType.Popularity:
                    comparator = p => p.QuantitySold;
                    break;
                case Consts.SortType.Rating:
                    comparator = p => p.AverageRating;
                    break;
                case Consts.SortType.Price:
                    comparator = p => -p.DiscountedPrice;
                    break;
                case Consts.SortType.TotalEarning:
                    comparator = p => p.TotalEarning;
                    break;
            }

            Collection = Collection.OrderByDescending(comparator);
            return this;
        }
        public ProductSearchHelper FromCategory(int categoryId)
        {
            if (categoryId != Consts.NoPreference)
            {
                Collection = Collection.Where(pro => pro.Category.Id == categoryId);
            }
            return this;
        }
        public ProductSearchHelper FromPrice(double minimumPrice)
        {
            if (Math.Abs(minimumPrice - Consts.NoPreference) > 0)
            {
                Collection = Collection.Where(pro => pro.DiscountedPrice >= minimumPrice);
            }
            return this;
        }
        public ProductSearchHelper ToPrice(double maximumPrice)
        {
            if (Math.Abs(maximumPrice - Consts.NoPreference) > 0)
            {
                Collection = Collection.Where(pro => pro.DiscountedPrice <= maximumPrice);
            }
            return this;
        }
        public ProductSearchHelper WithStatus(int status)
        {
            Collection = Collection.Where(pro => pro.Status == status);
            return this;
        }

        public ProductSearchHelper FromCategory(string category)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                Collection = Collection.Where(pro => pro.Category.Name.Equals(category, StringComparison.CurrentCultureIgnoreCase));
            }
            return this;
        }

        public async Task<Product> WithId(int id)
        {
            return await Collection.SingleOrDefaultAsync(pro => pro.Id == id);
        }

        public ProductSearchHelper HasBrand(ICollection<int> brands)
        {
            if (brands != null && brands.Any())
            {
                Collection = Collection.Where(pro => brands.Contains(pro.Brand.Id));
            }
            return this;
        }
        public ProductSearchHelper IncludeHidden(bool includeHidden)
        {
            if (includeHidden)
            {
                Collection = Collection.Where(pro => pro.Status != (int) Consts.Status.Deleted);
            }
            else
            {
                Collection = Collection.Where(pro => pro.Status == (int) Consts.Status.Public);
            }
            return this;
        }
    }

    public interface IProductRepository
    {
        Task<Product> FindProductById(int id);
        Task<Product> FindProductByUrl(string url);
        void ClearCache(Product p);
        Task AddCoupon(int id, Coupon coupon);
        Task RemoveCoupon(int id, Coupon c);
        Task RemoveImage(int id, Image image);
        Task RemoveExtra(int id, int extraId);
        Task<Extra> UpdateExtra(int productId, Extra extra);
        Task Update(Product product);
        Task<bool> DeleteProduct(int id);
        Task<bool> CanReview(int id);
        Task<int> AddReview(int productId, int rating, string content);
        Task DeleteReview(int reviewId);
        Task<ICollection<Product>> SearchTopProducts(int top);
        Task<ICollection<Product>> SearchProducts();

        Task<ICollection<Product>> SearchProducts(int page, int pageSize, string category, double minPrice,
            double maxPrice, int status, ICollection<string> origins, ICollection<string> materials,
            ICollection<int> brands, int sortType, bool includeHidden);

        Task<Product> AddProduct(Product product);
        Product UpdateProduct(Product product);

        Task<int> CountProducts(string category, double minPrice, double maxPrice, int status,
            ICollection<string> origins, ICollection<string> materials, ICollection<int> brands, bool includeHidden);

        Task<ICollection<string>> GetOrigins(int top);
        Task<ICollection<string>> GetMaterials(int top);
        Task<Product> FindProductByIdForCart(int id);
        Task<Product> FindProductByIdForCartNoFreshen(int id);
        Task<ICollection<Brand>> GetBrands();
        Task<string> GenerateNameUrl(string name);

        Task<int> CreateProduct(string name, string nameUrl, int brand, int category, string origin, string material, double price,
            bool hasDiscount, double discountedPrice, string description, string features);

        Task<Product> FindActiveProductByUrl(string url);
        Task AddImages(int productId, ICollection<Image> images);
        Task<double> GetEarningThisMonth();
        Task<double> GetEarningToday();
        Task<double> FindPercentageOfMethod(Consts.PaymentMethod cashOnDelivery);
        Task<ICollection<Category>> GetCategoryDistribution();
        Task<ICollection<double>> GetEarningInPeriod(TimeSpan fromDays);
        Task<int> CountProducts();
        Task<int> SendEmail(int productId);
    }

    public class ProductRepository: IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IIdentityRepository _identityRepository;
        private readonly EmailSender _emailSender;

        public ProductRepository(ApplicationDbContext ctx, IMemoryCache cache, IIdentityRepository identityRepository, EmailSender emailSender)
        {
            _context = ctx;
            _cache = cache;
            _identityRepository = identityRepository;
            _emailSender = emailSender;
        }

        public async Task<Product> FindProductById(int id)
        {
            var cacheKey = Consts.GetCachePrefix(Consts.CachePrefix.ProductId, id);
            if (_cache.TryGetValue(cacheKey, out var product))
            {
                return FreshenProduct(product as Product);
            }
            else
            {
                var newProduct = FreshenProduct(await _context.Products.FindAsync(id));
                _cache.Set(cacheKey, newProduct);
                return newProduct;
            }
        }

        private Product FreshenProduct(Product newProduct)
        {
            newProduct.Extras = newProduct.Extras?.Where(p => !p.Deleted).ToList();
            newProduct.Extras?.ForEach(ext => { ext.Options = ext.Options.Where(opt => !opt.Deleted).ToList(); });
            newProduct.Coupons = newProduct.Coupons?.Where(c => !c.Deleted).ToList();
            newProduct.Reviews = newProduct.Reviews?.Where(r => !r.Deleted).ToList();
            newProduct.Reviews = newProduct.Reviews?.Where(r => !r.Deleted).ToList();
            return newProduct;
        }

        public async Task<Product> FindProductByUrl(string url)
        {
            var cacheKey = Consts.GetCachePrefix(Consts.CachePrefix.ProductUrl, url);
            if (_cache.TryGetValue(cacheKey, out var product))
            {
                return FreshenProduct(product as Product);
            }

            var newProduct = await _context.Products
                .Where(p => p.Status != (int)Consts.Status.Deleted)
                .Include(p => p.Category)
//                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .Include(p => p.Extras)
                .ThenInclude(extra => extra.Options)
                .FirstOrDefaultAsync(p => p.NameUrl.Equals(url, StringComparison.CurrentCultureIgnoreCase));
            if (newProduct != null)
            {
                newProduct = FreshenProduct(newProduct);
            }
            _cache.Set(cacheKey, newProduct);
            return newProduct;
        }

        public void ClearCache(Product p)
        {
            var cacheIdKey = Consts.GetCachePrefix(Consts.CachePrefix.ProductId, p.Id);
            var cacheIdUrl = Consts.GetCachePrefix(Consts.CachePrefix.ProductUrl, p.NameUrl);
            _cache.Remove(cacheIdKey);
            _cache.Remove(cacheIdUrl);
        }

        public async Task AddCoupon(int id, Coupon coupon)
        {
            var product = await _context.Products.Include(p => p.Coupons).FirstOrDefaultAsync(p => p.Id == id);
            product.Coupons.Add(coupon);
            coupon.Id = 0;
            await _context.SaveChangesAsync();
            ClearCache(product);
        }

        public async Task RemoveCoupon(int id, Coupon c)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            var coupon = await _context.Coupon.FindAsync(c.Id);
            coupon.Deleted = true;
            await _context.SaveChangesAsync();
            ClearCache(product);
        }

        public async Task RemoveImage(int id, Image image)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            product.Images = product.Images.Where(c => !c.Url.Equals(image.Url)).ToList();
            await _context.SaveChangesAsync();
            ClearCache(product);
        }

        public async Task RemoveExtra(int id, int extraId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            var extra = await _context.Extra.FindAsync(extraId);
            extra.Deleted = true;
            await _context.SaveChangesAsync();
            ClearCache(product);
        }

        public async Task<Extra> UpdateExtra(int productId, Extra extra)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            var ext = await _context.Extra.AnyAsync(e => e.Id == extra.Id);
            if (ext)
            {
                _context.Entry(extra).State = EntityState.Modified;
                extra.Options.ForEach(o =>
                {
                    _context.Entry(o).State =  o.Id == 0 ? EntityState.Added : EntityState.Modified;
                });
            }
            else
            {
                extra.Product = product;
                await _context.Extra.AddAsync(extra);
            }
            await _context.SaveChangesAsync();
            ClearCache(product);
            return extra;
        }

        public async Task Update(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            ClearCache(product);
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                product.Status = (int)Consts.Status.Deleted;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> CanReview(int id)
        {
            var currentUser = await _identityRepository.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return false;
            }

            var product = await _context.Products
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .Include(p => p.OrderItems)
                    .ThenInclude(o => o.Order)
                        .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            var hasBought = product.OrderItems
                .Select(o => o.Order)
                .Where(o => o.Refund == null)
                .Select(o => o.User.Id)
                .Any(i => i.Equals(currentUser.Id));
            var hasReviewed = product.Reviews.Where(r => !r.Deleted).Select(r => r.User.Id).Contains(currentUser.Id);

            return hasBought && !hasReviewed;
        }

        public async Task<int> AddReview(int productId, int rating, string content)
        {
            var product = await _context.Products.FindAsync(productId);
            var currentUser = await _identityRepository.GetCurrentUserAsync();
            var review = new Review
            {
                Product = product,
                Rating = rating,
                Content = content,
                DateCreated = DateTime.Now,
                User = currentUser
            };

            await _context.Review.AddAsync(review);
            await _context.SaveChangesAsync();
            ClearCache(product);
            return review.Id;
        }

        public async Task DeleteReview(int reviewId)
        {
            var review = await _context.Review.FindAsync(reviewId);
            review.Deleted = true;
            await _context.SaveChangesAsync();
        }
        public async Task<ICollection<Product>> SearchTopProducts(int top)
        {
            var searcher = new ProductSearchHelper(_context);
            var col = await searcher
                .IncludeImages()
                .IncludeOrderItems()
                .Get();
            col.ForEach(p =>
            {
                p.TotalEarningBackup = p.TotalEarning;
                p.QuantitySoldBackup = p.QuantitySold;
                p.OrderItems = null;
                p.Brand = null;
                p.Category = null;
            });
            col = col.OrderByDescending(p => p.TotalEarningBackup)
                .Where(p => p.TotalEarningBackup > 0)
                .Take(top).ToList();
            return col;
        }

        public async Task<ICollection<Product>> SearchProducts() => await _context.Products.ToListAsync();
        public async Task<ICollection<Product>> SearchProducts(int page, int pageSize, string category, double minPrice,
            double maxPrice, int status, ICollection<string> origins, ICollection<string> materials,
            ICollection<int> brands, int sortType, bool includeHidden)
        {
            var searcher = new ProductSearchHelper(_context);
            return await searcher
                .FromCategory(category)
                .FromPrice(minPrice)
                .ToPrice(maxPrice)
                .IncludeHidden(includeHidden)
                .HasOrigin(origins)
                .HasMaterial(materials)
                .HasBrand(brands)
                .SortBy((Consts.SortType) sortType)
                .IncludeBrands()
                .IncludeCategory()
                .IncludeImages()
                .Page(page, pageSize)
                .Get();
        }
        public async Task<Product> AddProduct(Product product)
        {
            var entityEntry = await _context.Products.AddAsync(product);
            return entityEntry.Entity;
        }
        public Product UpdateProduct(Product product)
        {
            var entityEntry = _context.Products.Update(product);
            return entityEntry.Entity;
        }
        public async Task<int> CountProducts(string category, double minPrice, double maxPrice, int status,
            ICollection<string> origins, ICollection<string> materials, ICollection<int> brands, bool includeHidden)
        {
            var searcher = new ProductSearchHelper(_context);
            return await searcher
                .FromCategory(category)
                .FromPrice(minPrice)
                .ToPrice(maxPrice)
                .IncludeHidden(includeHidden)
                .HasOrigin(origins)
                .HasMaterial(materials)
                .HasBrand(brands)
                .Count();
        }
        public async Task<ICollection<string>> GetOrigins(int top)
        {
            return await _context.Products
                .Where(pro => !string.IsNullOrEmpty(pro.Origin))
                .Select(pro => pro.Origin)
                .Distinct()
                .Take(top)
                .ToListAsync();
        }
        public async Task<ICollection<string>> GetMaterials(int top)
        {
            return await _context.Products
                .Where(pro => !string.IsNullOrEmpty(pro.Material))
                .Select(pro => pro.Material)
                .Distinct()
                .Take(top)
                .ToListAsync();
        }

        public async Task<Product> FindProductByIdForCart(int id)
        {
            var helper = new ProductSearchHelper(_context);
            var p = await helper
                .IncludeImages()
                .IncludeExtras()
                .IncludeCoupons()
                .WithId(id);
            return FreshenProduct(p);
        }

        public async Task<Product> FindProductByIdForCartNoFreshen(int id)
        {
            var helper = new ProductSearchHelper(_context);
            var p = await helper
                .IncludeImages()
                .IncludeExtras()
                .IncludeCoupons()
                .WithId(id);
            return p;
        }

        public async Task<ICollection<Brand>> GetBrands()
        {
            return await _context.Brand.Where(brand => !brand.Deleted).OrderBy(brand => brand.Name).ToListAsync();
        }

        public async Task<string> GenerateNameUrl(string name)
        {
            return await GenerateNameUrl(name, 0);
        }

        public async Task<int> CreateProduct(string name, string nameUrl, int brand, int category, string origin, string material, double price,
            bool hasDiscount, double discountedPrice, string description, string features)
        {
            var brandObj = await _context.Brand.FindAsync(brand);
            var categoryObj = await _context.Category.FindAsync(category);
            var currentUser = await _identityRepository.GetCurrentUserAsync();
            var newProduct = new Product
            {
                Name = name,
                NameUrl = nameUrl,
                Brand = brandObj,
                Category = categoryObj,
                Description = description,
                Features = features,
                Price = price,
                Creator = currentUser,
                DateCreated = DateTime.Now,
                Origin = origin,
                Material = material,
                DiscountedPrice = hasDiscount ? discountedPrice : price
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            return newProduct.Id;
        }

        public async Task<Product> FindActiveProductByUrl(string url)
        {
            var cacheKey = Consts.GetCachePrefix(Consts.CachePrefix.ProductUrl, url);
            if (_cache.TryGetValue(cacheKey, out var product))
            {
                return FreshenProduct(product as Product);
            }

            var newProduct = await _context.Products
                .Where(p => p.Status != (int)Consts.Status.Deleted)
                .Include(p => p.Coupons)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Extras)
                    .ThenInclude(extra => extra.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.NameUrl.Equals(url, StringComparison.CurrentCultureIgnoreCase));
            newProduct = FreshenProduct(newProduct);
            _cache.Set(cacheKey, newProduct);
            return newProduct;
        }

        public async Task AddImages(int productId, ICollection<Image> images)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            images.ForEach(img =>
            {
                product.Images.Add(img);
            });
            await _context.SaveChangesAsync();
            ClearCache(product);
        }

        public async Task<double> GetEarningThisMonth()
        {
            var bug = await _context.Orders
                .Include(o => o.OrderCoupons)
                .ThenInclude(oc => oc.Coupon)
                .ThenInclude(c => c.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
                .ThenInclude(oio => oio.Option)
                .Include(o => o.Refund)
                .Where(o => o.DateCreated.AddMonths(1) >= DateTime.Now)
                .ToListAsync();
            return bug.Sum(b => b.PostCouponTotal);
        }

        public async Task<double> GetEarningToday()
        {
            return await GetEarningThisDay(DateTime.Now);
        }
        public async Task<int> CountProducts()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<int> SendEmail(int productId)
        {
            var users = await _identityRepository.GetSubscribedUsers();
            var emails = users.Select(u => u.Email).ToList();
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            var order = await _context.Orders
                .Include(o => o.OrderCoupons)
                .ThenInclude(oc => oc.Coupon)
                .ThenInclude(c => c.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
                .ThenInclude(oio => oio.Option)
                .ThenInclude(opt => opt.Extra)
                .Include(o => o.User)
                .Include(o => o.Refund)
                .FirstAsync();
            var result = await _emailSender.SendProductEmail($"[Kalium] {product.Name}", product, emails);
            return result ? users.Count : -1;
        }

        public async Task<double> GetEarningThisDay(DateTime date)
        {
            var n = (await _context.Orders
                .Include(o => o.OrderCoupons)
                .ThenInclude(oc => oc.Coupon)
                .ThenInclude(c => c.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemOptions)
                .ThenInclude(oio => oio.Option)
                .ThenInclude(opt => opt.Extra)
                .Include(o => o.Refund)
                .Where(o => o.DateCreated.Date == date.Date)
                .ToListAsync());
            
            return n.Sum(b => b.PostCouponTotal);
        }

        public async Task<double> FindPercentageOfMethod(Consts.PaymentMethod method)
        {
            var total = await _context.Orders.CountAsync();
            var methodCount = await _context.Orders
                .Where(o => o.PaymentMethod == (int) method)
                .CountAsync();
            return methodCount * 100.0 / total;
        }

        public async Task<ICollection<Category>> GetCategoryDistribution()
        {
            var catHelper = new CategorySearchHelper(_context);
            return await catHelper.Active().IncludeCountHidden().Get();
        }

        public async Task<ICollection<double>> GetEarningInPeriod(TimeSpan fromDays)
        {
            var earnings = new List<Double>();
            foreach (var day in Enumerable.Range(1, fromDays.Days).Reverse())
            {
                var date = DateTime.Today - TimeSpan.FromDays(day);
                earnings.Add(await GetEarningThisDay(date));
            }

            return earnings;
        }

        private async Task<string> GenerateNameUrl(string name, int index)
        {
            var normalizedName = name
                .Trim()
                .ToLower()
                .Normalize()
                .Replace(" ", "-") + (index > 0 ? $"index-{index}" : "");
            if (await _context.Products.AnyAsync(p => p.NameUrl.Equals(normalizedName)))
            {
                return await GenerateNameUrl(name, index + 1);
            }

            return normalizedName;
        }
    }
}

