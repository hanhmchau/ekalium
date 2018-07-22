using Kalium.Server.Context;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Kalium.Server.Repositories;
using Kalium.Server.HubR;
using Kalium.Shared.Consts;
using Kalium.Shared.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Kalium.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(@"Server=CHAUNHMSE63147\SQLEXPRESS;Database=KaliumApp;Trusted_Connection=True;MultipleActiveResultSets=true"));
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 4;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddAuthentication();
//                .AddGoogle(googleOptions =>
//                {
//                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
//                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
//                });
            services.ConfigureApplicationCookie(options =>
            {
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = ReplaceRedirectorWithStatusCode(HttpStatusCode.Forbidden),
                    OnRedirectToLogin = ReplaceRedirectorWithStatusCode(HttpStatusCode.Unauthorized)
                };

                options.Cookie.Name = ".kalium"; // ".applicationname"
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.SlidingExpiration = true;
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader()
                        .AllowAnyOrigin()
                        .AllowCredentials();
                }));

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Consts.Policy.ManageProducts.Name(), policy => policy.RequireClaim(ClaimTypes.Name, "ProductManager"));
                options.AddPolicy(Consts.Policy.ManageSocial.Name(), policy => policy.RequireClaim(ClaimTypes.Name, "SocialManager"));
                options.AddPolicy(Consts.Policy.ManageUser.Name(), policy => policy.RequireClaim(ClaimTypes.Name, "UserManager"));
                options.AddPolicy(Consts.Policy.Checkout.Name(), policy => policy.RequireAuthenticatedUser());
                options.AddPolicy(Consts.Policy.Auction.Name(), policy => policy.RequireAuthenticatedUser());
            });

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });
            services.AddTransient<IIdentityRepository, IdentityRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ICheckoutRepository, CheckoutRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IImageRepository, ImageRepository>();
            services.AddTransient<IBrandRepository, BrandRepository>();

            services.AddTransient<IProductHub, ProductHub>();
            services.AddTransient<IFetcher, Fetcher>();

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });

            services.AddResponseCaching();

            services.AddSingleton<EmailSender, EmailSender>();
//
//            services
//                .Configure<AuthMessageSenderOptions>(Configuration.GetSection(nameof(AuthMessageSenderOptions)))
//                .AddOptions()
//                .BuildServiceProvider();

            services.Configure<AuthMessageSenderOptions>(optionsSetup =>
                {
                    optionsSetup.SendGridUser = Configuration["AuthMessageSenderOptions:SendGridUser"];
                    optionsSetup.SendGridKey = Configuration["AuthMessageSenderOptions:SendGridKey"];
                });
        }

        static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirectorWithStatusCode(HttpStatusCode statusCode) => context =>
        {
            // Adapted from https://stackoverflow.com/questions/42030137/suppress-redirect-on-api-urls-in-asp-net-core
            context.Response.StatusCode = (int)statusCode;
            return Task.CompletedTask;
        };

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseCors("CorsPolicy");

            app.UseSignalR(routes => {
                routes.MapHub<ProductHub>("/productHubber");
            });

            app.UseResponseCaching();

//            app.Use(async (context, next) =>
//            {
//                context.Response.GetTypedHeaders().CacheControl =
//                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue
//                    {
//                        Public = true,
//                        MaxAge = TimeSpan.FromSeconds(10)
//                    };
//                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
//                    new[] { "Accept-Encoding" };
//
//                await next();
//            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}");
            });

            app.UseBlazor<Client.Program>();
        }
    }
}
