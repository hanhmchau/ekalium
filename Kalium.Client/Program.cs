using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Kalium.Client.Extensions;
using Kalium.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Kalium.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(services =>
            {
                // Add any custom services here
                services.AddTransient<IFetcher, Fetcher>();
                services.AddStorage();
                services.AddToastr();
                services.AddSingleton<IUtil, Util>();
                services.AddTransient<IHttpApiClientRequestBuilder, HttpApiClientRequestBuilder>();
                services.AddLogging();
                services.AddTransient<IHttpApiClientRequestBuilderFactory, HttpApiClientRequestBuilderFactory>();
                services.AddSingleton<IAccountService, AccountService>();
                services.AddSingleton<IMegaService, MegaService>();
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
