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
        Task<bool> HasOrderById(int id);
        Task<Refund> CancelOrder(Order order);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext ctx)
        {
            _context = ctx;
        }
        public async Task<bool> HasOrderById(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }

        private static double RefundRate(Order order)
        {
            switch ((Consts.OrderStatus) order.Status)
            {
                case Consts.OrderStatus.Processing:
                    var daysApart = DateTime.Now.Subtract(order.DateCreated).Days;
                    if (daysApart < 3)
                    {
                        return 0.9;
                    }

                    if (daysApart < 7)
                    {
                        return 0.7;
                    }

                    return 0.5;
                case Consts.OrderStatus.Delivering:
                    return 0.3;
            }

            return 0;
        }

        public async Task<Refund> CancelOrder(Order order)
        {
            var refund = new Refund
            {
                DateRefunded = DateTime.Now,
                RefundRate = RefundRate(order)
            };

            order.Refund = refund;
            order.Status = (int) Consts.OrderStatus.Cancelled;

            await _context.SaveChangesAsync();
            return refund;
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
                .Include(o => o.Refund)
                .FirstOrDefaultAsync(o => o.Id == id);
            return order;
        }
    }
}
