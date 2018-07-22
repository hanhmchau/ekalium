using System;
using Kalium.Server.Utils;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kalium.Server.Repositories;
using Kalium.Shared;
using Kalium.Shared.Consts;
using Kalium.Shared.Front;
using Microsoft.AspNetCore.Hosting;
using MoreLinq;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _iProductRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IIdentityRepository _identityRepository;
        private readonly IOrderRepository _iOrderRepository;
        private readonly IBrandRepository _brandRepository;

        public ProductController(IProductRepository productRepository, IHostingEnvironment environment, IImageRepository imageRepository, ICategoryRepository categoryRepository, IIdentityRepository identityRepository, IOrderRepository iOrderRepository, IBrandRepository brandRepository)
        {
            this._iProductRepository = productRepository;
            _imageRepository = imageRepository;
            _categoryRepository = categoryRepository;
            _identityRepository = identityRepository;
            _iOrderRepository = iOrderRepository;
            _brandRepository = brandRepository;
        }

        [HttpPost("[action]")]
        public async Task<string> LoadProducts([FromBody] string json)
        {
            var parser = new Parser(json);
            int page = parser.AsInt("Page");
            string category = HttpUtility.UrlDecode(parser.AsString("CategoryName"));
            double minPrice = parser.AsDouble("MinPrice");
            double maxPrice = parser.AsDouble("MaxPrice");
            int sortType = parser.AsInt("SortType");
            int pageSize = parser.AsInt("PageSize");
            int status = (int) Consts.Status.Public;
            var origins = parser.AsObject<ICollection<string>>("ChosenOrigins");
            var materials = parser.AsObject<ICollection<string>>("ChosenMaterials");
            var brands = parser.AsObject<ICollection<int>>("ChosenBrands");
            var includeHidden = parser.AsBool("IncludeHidden");

            var products = await _iProductRepository.SearchProducts(page, pageSize, category, minPrice, maxPrice, status, origins, materials, brands, sortType, includeHidden);
            var total = await _iProductRepository.CountProducts(category, minPrice, maxPrice, status, origins, materials, brands, includeHidden);
            object result = new
            {
                Products = products,
                Total = total
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

            var result = await _iProductRepository.DeleteProduct(id);
            return JsonConvert.SerializeObject(new
            {
                Result = result
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> LoadAttributes()
        {
            const int top = Consts.AttributeTop;

            var origins = await _iProductRepository.GetOrigins(top);
            var materials = await _iProductRepository.GetMaterials(top);
            var brands = await _iProductRepository.GetBrands();
            object result = new
            {
                Origins = origins,
                Materials = materials,
                Brands = brands
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> LoadBrands()
        {
            var brands = await _iProductRepository.GetBrands();
            object result = new
            {
                Brands = brands
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> GetProductByUrl([FromQuery] string url)
        {
            var product = await _iProductRepository.FindProductByUrl(url);
            if (product != null)
            {
                product.Category.Products.Clear();
                product.Extras.ForEach(ext =>
                {
                    ext.Product = null;
                    ext.Options.ForEach(opt => opt.Extra = null); 
                });
            }

            var result = new
            {
                Product = product
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include
            });
        }

        [HttpGet("[action]")]
        public async Task<string> GetProductByUrlForAdmin([FromQuery] string url)
        {
            var product = await _iProductRepository.FindActiveProductByUrl(url);
            if (product != null)
            {
                product.Category.Products.Clear();
                product.Brand.Products.Clear();
                product.Coupons?.ForEach(c => { c.Product = null; });
                product.Extras.ForEach(ext =>
                {
                    ext.Product = null;
                    ext.Options.ForEach(opt => opt.Extra = null);
                });
            }
            var result = new
            {
                Product = product
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<ICollection<Image>> UploadImages([FromForm] ImageViewModel model)
        {
            ICollection<Image> images = new List<Image>();
            var files = model.Files;
            if (files != null && files.Any())
            {
                images = _imageRepository.Save(files, Consts.Folder.Product.Name());
                await _iProductRepository.AddImages(model.Id, images);
            }

            return images;
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<string> AddCoupon([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("Id");
            var coupon = parser.AsObject<Coupon>("Coupon");
            await _iProductRepository.AddCoupon(id, coupon);
            return JsonConvert.SerializeObject(new
            {
                Result = true
            });
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<string> RemoveCoupon([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("Id");
            var coupon = parser.AsObject<Coupon>("Coupon");
            await _iProductRepository.RemoveCoupon(id, coupon);
            return JsonConvert.SerializeObject(new
            {
                Result = true
            });
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<string> RemoveExtra([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("Id");
            var extra = parser.AsInt("Extra");
            await _iProductRepository.RemoveExtra(id, extra);
            return JsonConvert.SerializeObject(new
            {
                Result = true
            });
        }
        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<string> UpdateExtra([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("Id");
            var extra = parser.AsObject<Extra>("Extra");
            var newExtra = await _iProductRepository.UpdateExtra(id, extra);
            return JsonConvert.SerializeObject(new
            {
                Result = true,
                Extra = newExtra
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }


        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<string> RemoveImage([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("Id");
            var image = parser.AsObject<Image>("Image");
            await _iProductRepository.RemoveImage(id, image);
            return JsonConvert.SerializeObject(new
            {
                Result = true
            });
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<bool> Update([FromBody] Product product)
        {
            await _iProductRepository.Update(product);
            return true;
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.ManageProducts)]
        public async Task<string> CreateProduct([FromBody] string json)
        {
            var parser = new Parser(json);
            var name = parser.AsString("Name");
            var nameUrl = await _iProductRepository.GenerateNameUrl(name);
            var brand = parser.AsInt("Brand");
            var category = parser.AsInt("Category");
            var origin = parser.AsString("Origin");
            var material = parser.AsString("Material");
            var price = parser.AsDouble("Price");
            var hasDiscount = parser.AsBool("HasDiscount");
            var discountedPrice = hasDiscount ? parser.AsDouble("DiscountedPrice") : price;

            var description = parser.AsString("Description");
            var features = parser.AsString("Features");

            var newId = await _iProductRepository.CreateProduct(name, nameUrl, brand, category,
                origin, material, price, hasDiscount, discountedPrice, description, features);

            return JsonConvert.SerializeObject(new 
            {
                Id = newId,
                Url = nameUrl
            });
        }

        [HttpPost("[action]")]
        public async Task<bool> CanReview([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("ProductId");
            return await _iProductRepository.CanReview(id);
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.Checkout)]
        public async Task<int> AddReview([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("ProductId");
            var rating = parser.AsInt("Rating");
            var content = parser.AsString("Content");
            return await _iProductRepository.AddReview(id, rating, content);
        }

        [HttpPost("[action]")]
        [AuthorizePolicies(Consts.Policy.Checkout)]
        public async Task<bool> DeleteReview([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("ReviewId");
            await _iProductRepository.DeleteReview(id);
            return true;
        }

        [HttpGet("[action]")]
        public async Task<string> Main()
        {
            return JsonConvert.SerializeObject(new
            {
                TopProducts = await _iProductRepository.SearchTopProducts(6),
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> GetDashboardPieOrder()
        {
            var countProduct = await _iProductRepository.CountProducts();
            var categoryDistribution = await _iProductRepository.GetCategoryDistribution();
            categoryDistribution.ForEach(c => { c.Products = new List<Product>(); });
            var lastOrders = await _iOrderRepository.SearchLatestOrder(5);
            lastOrders.ForEach(o =>
            {
                o.PostCouponTotalBackup = o.PostCouponTotal;
                o.OrderItems = new List<OrderItemData>();
                o.Coupons = new List<CouponData>();
            });
            var newCategories = new List<Category>();
            categoryDistribution.ForEach(c =>
            {
                newCategories.Add(new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.ProductCount
                });
            });
            return JsonConvert.SerializeObject(new
            {
                CategoryDistribution = newCategories,
                LastOrders = lastOrders,
                CountProduct = countProduct
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> GetDashboardChart()
        {
            var earningThisMonth = await _iProductRepository.GetEarningThisMonth();
            var earningToday = await _iProductRepository.GetEarningToday();
            var percentageCash = await _iProductRepository.FindPercentageOfMethod(Consts.PaymentMethod.CashOnDelivery);
            var percentagePayPal = await _iProductRepository.FindPercentageOfMethod(Consts.PaymentMethod.PayPal);
            var earningLastWeek = await _iProductRepository.GetEarningInPeriod(TimeSpan.FromDays(7));
            return JsonConvert.SerializeObject(new
            {
                EarningThisMonth = earningThisMonth,
                EarningToday = earningToday,
                PercentageCash = percentageCash,
                PercentagePayPal = percentagePayPal,
                EarningLastWeek = earningLastWeek
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> GetDashboardTopsProducts()
        {
            var topProducts = await _iProductRepository.SearchTopProducts(5);
            var topCategories = await _categoryRepository.SearchTopCategories(5);
            return JsonConvert.SerializeObject(new
            {
                TopProducts = topProducts,
                TopCategories = topCategories
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet("[action]")]
        public async Task<string> GetDashboardTopsUsers()
        {
            var topUsers = await _identityRepository.SearchTopBoughtUsers(5);
            topUsers.ForEach(u =>
            {
                u.TotalPaidBackup = u.TotalPaid;
                u.Orders = null;
            });
            var topBrands = await _brandRepository.SearchTopBrands(5);
            return JsonConvert.SerializeObject(new
            {
                TopUsers = topUsers,
                TopBrands = topBrands
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> EmailToSubscribers([FromBody] string json)
        {
            var parser = new Parser(json);
            var productId = parser.AsInt("ProductId");
            var count = await _iProductRepository.SendEmail(productId);
            return JsonConvert.SerializeObject(new
            {
                Count = count
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

    }
}

