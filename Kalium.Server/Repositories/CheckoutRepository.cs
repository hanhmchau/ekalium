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
    public interface ICheckoutRepository
    {
        Task<CheckOutResult> CheckOut(PseudoCart pseudoCart, string name, string address, string phone,
            bool sendToDifferentAddress, string alternateName, string alternateAddress, string alternatePhone,
            int paymentMethod, string note);
    }

    public class CheckoutRepository : ICheckoutRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly IIdentityRepository _identityRepository;
        private readonly EmailSender _emailSender;

        public CheckoutRepository(ApplicationDbContext ctx, IMemoryCache cache, IProductRepository productRepository, IIdentityRepository identityRepository, EmailSender emailSender)
        {
            _context = ctx;
            _productRepository = productRepository;
            _identityRepository = identityRepository;
            _emailSender = emailSender;
        }

        private async Task<CheckOutResult> ValidateCart(PseudoCart cart)
        {
            ICollection<string> messages = new List<string>();
            ECart realCart = new ECart();
            foreach (var item in cart.Cart.ToList())
            {
                bool valid = true;
                var upToDateItem = await _productRepository.FindProductByIdForCartNoFreshen(item.Id);
                ExtraDictionary choices = new ExtraDictionary();
                if (upToDateItem.Status != (int) Consts.Status.Public)
                {
                    messages.Add($"{upToDateItem.Name} is no longer on sale.");
                    valid = false;
                }
                else if (upToDateItem.Quantity < item.Quantity)
                {
                    messages.Add($"{upToDateItem.Name} does not have enough item on sale.");
                    valid = false;
                }
                else
                {
                    foreach (var choice in item.Choices)
                    {
                        bool validChoice = true;
                        var upToDateExtra =
                            upToDateItem.Extras.FirstOrDefault(ext => ext.Id == choice.Extra && !ext.Deleted);
                        Option upToDateOption = null;
                        if (upToDateExtra == null)
                        {
                            messages.Add($"An extra feature of product {upToDateItem.Name} does not exist.");
                            validChoice = false;
                        }
                        else
                        {
                            if (!upToDateExtra.Optional && choice.Option == null)
                            {
                                messages.Add($"All mandatory options were not fulfilled with product {upToDateItem.Name}.");
                                validChoice = false;
                            }
                            else
                            {
                                upToDateOption =
                                    upToDateExtra.Options.FirstOrDefault(opt => opt.Id == choice.Option && !opt.Deleted);
                                if (upToDateOption == null)
                                {
                                    messages.Add($"An option of product {upToDateItem.Name} does not exist.");
                                    validChoice = false;
                                }
                            }
                        }

                        if (validChoice)
                        {
                            choices.Add(upToDateExtra, upToDateOption);
                        }
                    }
                }

                if (valid)
                {
                    realCart.Contents.Add(new ECartItem
                    {
                        Product = upToDateItem,
                        Quantity = item.Quantity,
                        Choices = choices
                    });
                }
            }

            foreach (var coupon in cart.Coupons)
            {
                var realCoupon = realCart.AvailableCoupons.FirstOrDefault(c => c.Id == coupon && !c.Deleted && c.IsValid);
                if (realCoupon != null)
                {
                    realCart.Coupons.Add(realCoupon);
                } else
                {
                    messages.Add($"Coupon id ${coupon} is no longer valid and has been removed.");
                }
            }

            return new CheckOutResult
            {
                Messages = messages,
                ECart = realCart,
                Succeeded = realCart.Contents.Any()
            };
        }

        public async Task<CheckOutResult> CheckOut(PseudoCart cart, string name, string address, string phone,
            bool sendToDifferentAddress,
            string alternateName, string alternateAddress, string alternatePhone, int paymentMethod, string note)
        {
            var checkOutResult = await ValidateCart(cart);
            var currentUser = await _identityRepository.GetCurrentUserAsync();
            if (checkOutResult.Succeeded)
            {
                var realCart = checkOutResult.ECart;
                var newOrder = new Order
                {
                    User = currentUser,
                    BillingName = name,
                    Address = address,
                    PhoneNumber = phone,
                    ShipToDifferentAddress = sendToDifferentAddress,
                    PaymentMethod = paymentMethod,
                    Note = note,
                    DateCreated = DateTime.Now,
                    Status = (int) Consts.OrderStatus.Processing,
                    OrderCoupons = realCart.Coupons?.Select(c => new OrderCoupon
                    {
                        Coupon = c
                    }).ToList(),
                };

                newOrder.Coupons.ForEach(c =>
                {
                    if (c.Type == (int) Consts.CouponType.Quantity)
                    {
                        c.Quantity--; //use up one coupon
                    }
                });

                newOrder.OrderItems = realCart.Contents.Select(item => new OrderItem
                {
                    Product = item.Product,
                    Price = item.Product.DiscountedPrice,
                    Quantity = item.Quantity,
                    OrderItemOptions = item.Choices.Values.Select(o => new OrderItemOption
                    {
                        Option = o
                    }).ToList()
                }).ToList();
                if (sendToDifferentAddress)
                {
                    newOrder.AlternateName = alternateName;
                    newOrder.AlternateAddress = alternateAddress;
                    newOrder.AlternatePhone = alternatePhone;
                }

                await _context.Orders.AddAsync(newOrder);
                await _context.SaveChangesAsync();
                checkOutResult.OrderId = newOrder.Id;
            }

            if (checkOutResult.Succeeded)
            {
                var orderToEmail = await _context.Orders
                    .Include(o => o.OrderCoupons)
                    .ThenInclude(oc => oc.Coupon)
                    .ThenInclude(c => c.Product)
                    .ThenInclude(p => p.Images)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemOptions)
                    .ThenInclude(oio => oio.Option)
                    .ThenInclude(opt => opt.Extra)
                    .Include(o => o.User)
                    .Include(o => o.Refund).FirstOrDefaultAsync(o => o.Id == checkOutResult.OrderId);
                var response = await _emailSender.SendOrderEmail($"[Kalium] Order #{checkOutResult.OrderId}", orderToEmail,
                    currentUser.Email);
            }

            return checkOutResult;
        }
    }
}
