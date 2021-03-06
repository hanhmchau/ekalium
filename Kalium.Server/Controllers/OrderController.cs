﻿using System;
using Kalium.Server.Context;
using Kalium.Server.Utils;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Kalium.Server.Repositories;
using Kalium.Shared.Consts;
using Kalium.Shared.Conversions;
using Microsoft.AspNetCore.Authorization;
using MoreLinq;

namespace Kalium.Server.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IIdentityRepository _identityRepository;
        private readonly IAuthorizationService _authorizationService;

        public OrderController(IOrderRepository orderRepository, IIdentityRepository identityRepository, IAuthorizationService authorizationService)
        {
            _orderRepository = orderRepository;
            _identityRepository = identityRepository;
            _authorizationService = authorizationService;
        }
        
        [HttpGet("[action]")]
        [AuthorizePolicies(Consts.Policy.Checkout)]
        public async Task<string> FindOrder([FromQuery] int id)
        {
            var order = await _orderRepository.FindOrderById(id);
            var currentUser = await _identityRepository.GetCurrentUserAsync();

            if (order == null)
            {
                return SerializeObject(new
                {
                    Code = 404
                });
            }

            var isAuthorized =
                await _authorizationService.AuthorizeAsync(HttpContext.User, Consts.Policy.ManageProducts.Name());
            if (!currentUser.Id.Equals(order.User.Id) && !isAuthorized.Succeeded)
            {
                return SerializeObject(new
                {
                    Code = 403
                });
            }

            var simpleOrder = order.Simplify();
            var result = new
            {
                Code = 200,
                Order = simpleOrder
            };
            return SerializeObject(result);
        }

        [HttpGet("[action]")]
        [AuthorizePolicies(Consts.Policy.Checkout)]
        public async Task<string> Cancel([FromQuery] int id)
        {
            if (!await _orderRepository.HasOrderById(id))
            {
                return SerializeObject(new
                {
                    Code = 404
                });
            }
            var order = await _orderRepository.FindOrderById(id);

            var currentUser = await _identityRepository.GetCurrentUserAsync();
            var isAuthorized =
                await _authorizationService.AuthorizeAsync(HttpContext.User, Consts.Policy.ManageProducts.Name());
            if (!currentUser.Id.Equals(order.User.Id) && !isAuthorized.Succeeded)
            {
                return SerializeObject(new
                {
                    Code = 403
                });
            }

            var refund = await _orderRepository.CancelOrder(order);
//            refund.Order = null;
            var result = new
            {
                Code = 200,
                RefundDate = refund.DateRefunded,
                RefundRate = refund.RefundRate
            };
            return SerializeObject(result);
        }

        private static string SerializeObject(object result)
        {
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> UpdateStatus([FromBody] string json)
        {
            var parser = new Parser(json);
            var id = parser.AsInt("Id");
            var status = parser.AsInt("Status");
            await _orderRepository.UpdateOrderStatus(id, status);
            return JsonConvert.SerializeObject(new
            {
                Result = true
            }, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost("[action]")]
        public async Task<string> SearchOrders([FromBody] string json)
        {
            var parser = new Parser(json);
            var sortType = parser.AsInt("SortType");
            var page = parser.AsInt("Page");
            var pageSize = parser.AsInt("PageSize");
            var orderStatus = parser.AsInt("OrderStatus");
            var start = parser.AsString("StartDate");
            var end = parser.AsString("EndDate");
            var user = parser.AsString("User");
            if (!DateTime.TryParseExact(start, "d_MM_yyyy", null, DateTimeStyles.None,
                out DateTime startDate))
            {
                startDate = DateTime.MinValue;
            }
            if (!DateTime.TryParseExact(end, "d_MM_yyyy", null, DateTimeStyles.None,
                out DateTime endDate))
            {
                endDate = DateTime.MinValue;
            }
            object result = new
            {
                Orders = await _orderRepository.SearchOrders(orderStatus, startDate, endDate, user, sortType, page, pageSize),
                Total = await _orderRepository.CountOrders(orderStatus, startDate, endDate, user)
            };
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
