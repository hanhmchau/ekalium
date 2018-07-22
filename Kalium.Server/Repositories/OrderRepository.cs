using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kalium.Server.Context;
using Kalium.Shared.Consts;
using Kalium.Shared.Conversions;
using Kalium.Shared.Front;
using Kalium.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoreLinq;

namespace Kalium.Server.Repositories
{
    internal class OrderSearchHelper : SearchHelper<Order>
    {
        public OrderSearchHelper(ApplicationDbContext context) : base(context)
        {
            Collection = context.Orders;
        }
        
        public OrderSearchHelper IncludeProducts()
        {
            Collection = Collection
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
                .Include(o => o.Refund);
            return this;
        }

        public OrderSearchHelper SortBy(Consts.SortType sortType)
        {
            Expression<Func<Order, IComparable>> comparator = c => c.DateCreated;
            switch (sortType)
            {
                case Consts.SortType.Newness:
                    comparator = c => c.DateCreated;
                    break;
                case Consts.SortType.Price:
                    comparator = p => p.PostCouponTotal;
                    break;
            }

            Collection = Collection.OrderByDescending(comparator);
            return this;
        }

        public OrderSearchHelper WithStatus(int status)
        {
            if (status != -1)
            {
                Collection = Collection.Where(o => o.Status == status);
            }
            return this;
        }
        public OrderSearchHelper ByUser(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                Collection = Collection.Include(o => o.User).Where(o => o.User.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase));
            }
            return this;
        }

        public OrderSearchHelper Between(DateTime start, DateTime end)
        {
            if (start != DateTime.MinValue && end != DateTime.MinValue)
            {
                Collection = Collection.Where(o => o.DateCreated >= start && o.DateCreated <= end);
            }
            return this;
        }
    }

    public interface IOrderRepository
    {
        Task<Order> FindOrderById(int id);
        Task<bool> HasOrderById(int id);
        Task<Refund> CancelOrder(Order order);
        Task<ICollection<OrderData>> SearchOrders(int orderStatus, DateTime startDate, DateTime endDate, string user,
            int sortType, int page,
            int pageSize);

        Task<int> CountOrders(int orderStatus, DateTime startDate, DateTime endDate, string user);
        Task UpdateOrderStatus(int id, int status);
        Task<ICollection<OrderData>> SearchLatestOrder(int top);
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

        public async Task<ICollection<OrderData>> SearchOrders(int orderStatus, DateTime startDate, DateTime endDate,
            string user, int sortType, int page,
            int pageSize)
        {
            var orders = await new OrderSearchHelper(_context)
                .WithStatus(orderStatus)
                .Between(startDate, endDate)
                .IncludeProducts()
                .ByUser(user)
                .SortBy((Consts.SortType) sortType)
                .Page(page, pageSize)
                .Get();
            var orderDatas = orders.Select(o => o.Simplify()).ToList();
            return orderDatas;
        }

        public async Task<ICollection<OrderData>> SearchLatestOrder(int top)
        {
            var orders = await new OrderSearchHelper(_context)
                .IncludeProducts()
                .SortBy(Consts.SortType.Newness)
                .Page(1, top)
                .Get();
            var orderDatas = orders.Select(o => o.Simplify()).ToList();
            return orderDatas;
        }

        public async Task<int> CountOrders(int orderStatus, DateTime startDate, DateTime endDate, string user)
        {
            return await new OrderSearchHelper(_context)
                .WithStatus(orderStatus)
                .Between(startDate, endDate)
                .ByUser(user)
                .Count();
        }

        public async Task UpdateOrderStatus(int id, int status)
        {
            var order = await _context.Orders.Include(o => o.Refund).FirstOrDefaultAsync(o => o.Id == id);
            //if there is a refund and the new status is not-cancelled, delete that refund
            if (order.Refund != null && status != (int) Consts.OrderStatus.Cancelled)
            {
                _context.Refund.Remove(order.Refund);
            }

            //cancelling an order that wasn't previously cancelled
            if (status == (int)Consts.OrderStatus.Cancelled && order.Status != (int) Consts.OrderStatus.Cancelled)
            {
                await CancelOrder(order);
            }
            order.Status = status;
            await _context.SaveChangesAsync();
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
