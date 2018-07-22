using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor.Components;

namespace Kalium.Client.Pages
{
    public class IndexModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected ICollection<Category> TopCategories { get; set; }
        protected ICollection<Product> TopProducts { get; set; }

        protected override async Task OnInitAsync()
        {
            var result = await MegaService.Fetcher.Fetch("/api/Product/Main");
            TopProducts = result["TopProducts"].ToObject<ICollection<Product>>();
            StateHasChanged();
        }
    }
}
