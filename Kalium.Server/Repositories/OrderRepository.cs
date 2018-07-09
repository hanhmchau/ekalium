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
    public interface IOrderRepository
    {
        Task<Order> FindOrderById(int id);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public async Task<Order> FindOrderById(int id)
        {
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
                .FirstOrDefaultAsync(o => o.Id == id);
            return order;
        }
    }
}
