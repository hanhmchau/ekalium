using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Front;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Components;

namespace Kalium.Client.Admin
{
    public class DashboardModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        protected ICollection<OrderData> LatestOrders { get; set; } = new List<OrderData>();
        protected ICollection<User> TopUsers { get; set; } = new List<User>();
        protected ICollection<Product> TopProducts { get; set; } = new List<Product>();
        protected ICollection<Brand> TopBrands { get; set; } = new List<Brand>();
        protected ICollection<Category> TopCategories { get; set; } = new List<Category>();
        protected double PercentageCash { get; set; }
        protected double PercentagePayPal { get; set; }
        protected double EarningThisMonth { get; set; }
        protected double EarningToday { get; set; }
        protected ICollection<Category> CategoryDistribution { get; set; } = new List<Category>();
        protected ICollection<double> EarningLastWeek { get; set; } = new List<double>();
        public int CategoryCount { get; set; }
        protected override async Task OnInitAsync()
        {
            RegisteredFunction.Invoke<bool>("removeCss", "outside");

            var resultChart = await MegaService.Fetcher.Fetch("/api/Product/GetDashboardChart");
            var resultPie = await MegaService.Fetcher.Fetch("/api/Product/GetDashboardPieOrder");
            var catDis = resultPie["CategoryDistribution"].ToObject<ICollection<Category>>();
            CategoryCount = resultPie["CountProduct"].ToObject<int>();
            LatestOrders = resultPie["LastOrders"].ToObject<ICollection<OrderData>>();
            EarningLastWeek = resultChart["EarningLastWeek"].ToObject<ICollection<double>>();
            PercentageCash = resultChart["PercentageCash"].ToObject<double>();
            PercentagePayPal = resultChart["PercentagePayPal"].ToObject<double>();
            EarningThisMonth = resultChart["EarningThisMonth"].ToObject<double>();
            EarningToday = resultChart["EarningToday"].ToObject<double>();
            PreparePieChart(catDis);
            LoadChart();

            var resultTops1 = await MegaService.Fetcher.Fetch("/api/Product/GetDashboardTopsProducts");
            var resultTops2 = await MegaService.Fetcher.Fetch("/api/Product/GetDashboardTopsUsers");
            TopUsers = resultTops2["TopUsers"].ToObject<ICollection<User>>();
            TopProducts = resultTops1["TopProducts"].ToObject<ICollection<Product>>();
            TopBrands = resultTops2["TopBrands"].ToObject<ICollection<Brand>>();
            TopCategories = resultTops1["TopCategories"].ToObject<ICollection<Category>>();
        }

        private void PreparePieChart(ICollection<Category> catDis)
        {
            catDis = catDis.Where(c => c.ProductCount > 0).OrderByDescending(c => c.ProductCount).ToList();
            foreach (var cat in catDis.Take(4).Reverse())
            {
                CategoryDistribution.Add(cat);
                catDis.Remove(cat);
            }

            CategoryDistribution = CategoryDistribution.OrderByDescending(c => c.ProductCount).ToList();
            var remaining = catDis.Sum(c => c.ProductCount);
            if (remaining > 0)
            {
                CategoryDistribution.Add(new Category
                {
                    Name = "Others",
                    ProductCount = remaining
                });
            }
        }

        private void LoadChart()
        {
            var categoryPieChartLabels = CategoryDistribution.Select(c => c.Name);
            var categoryPieChartData = CategoryDistribution.Select(c => c.ProductCount);
            var earningLastWeekLabels = GetRecentDates();
            var earningLastWeekData = EarningLastWeek;
            RegisteredFunction.Invoke<bool>("initDashboard", categoryPieChartLabels,
                categoryPieChartData, earningLastWeekLabels, earningLastWeekData);
        }

        private ICollection<string> GetRecentDates()
        {
            var dates = new List<string>();
            foreach (var offset in Enumerable.Range(1, 7))
            {
                var date = DateTime.Today - TimeSpan.FromDays(offset);
                dates.Add(date.ToString("d/M"));
            }

            dates.Reverse();
            return dates;
        }
    }
}
