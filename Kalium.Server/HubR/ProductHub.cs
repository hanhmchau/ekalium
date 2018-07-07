using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Kalium.Server.HubR
{
    public interface IProductHub
    {
        Task AnnounceAdd(Product product);
        Task AnnounceUpdate(Product product);
        Task AnnounceDelete(Product product);
        Task SendMessage(string user, string message);
    }

    public class ProductHub : Hub, IProductHub
    {
        private readonly IHubContext<ProductHub> _context;

        public ProductHub(IHubContext<ProductHub> context)
        {
            _context = context;
        }

        private async Task Send(Consts.HubActivity method)
        {
            await _context.Clients.All.SendAsync(method.ToString().ToLower(), "");
        }

        private async Task Send(Consts.HubActivity method, Product product)
        {
            var productJson = JsonConvert.SerializeObject(product, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            await _context.Clients.All.SendAsync(method.ToString().ToLower(), productJson);
        }

        public async Task AnnounceAdd(Product product)
        {
            await Send(Consts.HubActivity.AddProduct, product);
        }

        public async Task AnnounceUpdate(Product product)
        {
            await Send(Consts.HubActivity.UpdateProduct, product);
        }
        public async Task AnnounceDelete(Product product)
        {
            await Send(Consts.HubActivity.RemoveProduct, product);
        }

        public async Task SendMessage(string user, string message)
        {
            try
            {
                await _context.Clients.All.SendAsync("ReceiveMessage", user, message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task AnnounceRefreshCart()
        {
            await Send(Consts.HubActivity.RefreshCart);
        }
    }
}
