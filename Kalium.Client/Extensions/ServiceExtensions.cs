using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Kalium.Client.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddToastr(this IServiceCollection col)
        {
            col.AddSingleton<Toastr>();
        }
    }
}
