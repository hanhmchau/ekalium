using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Kalium.Shared.Services;

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
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
