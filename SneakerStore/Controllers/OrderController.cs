using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Repository;
using Repository.Models;
using SneakerStore.Models;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Customer, Admin")]
    public class OrderController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;

        public OrderController(UserRepository userRepository, OrderRepository orderRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public IActionResult Index([FromQuery] int page, [FromQuery] byte status)
        {
            // Orders page
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get order list
            int size = 10;
            page = page == 0 ? 1 : page;
            var orderList = _orderRepository.GetOrdersByUserIdAndStatusPagination(long.Parse(userId.Value), page, size, status);
            int orderCount = _orderRepository.CountAllOrderByUserIdAndStatus(long.Parse(userId.Value), status);
            int totalPages = (int)Math.Ceiling((double)orderCount / size);
            List<int> pageNumbers = new List<int>();
            if (totalPages > 0)
            {
                int start = Math.Max(1, page - 2);
                int end = Math.Min(page + 2, totalPages);

                if (totalPages > 5)
                {
                    if (end == totalPages) start = end - 4;
                    else if (start == 1) end = start + 4;
                }
                else
                {
                    start = 1;
                    end = totalPages;
                }
                pageNumbers = Enumerable.Range(start, end - start + 1).ToList();
            }

            OrderListClientViewModel model = new()
            {
                OrdersPaginated = orderList,
                Size = size,
                Page = page,
                TotalCount = orderCount,
                TotalPage = totalPages,
                PageNumbers = pageNumbers,
                Status = status
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Detail(long id)
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userRole = claimsIdentity.FindFirst(ClaimTypes.Role);

            if (userId == null || userRole == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get order detail by id
            var order = _orderRepository.GetById(id);

            // Check if current user is owner of order
            if((long.Parse(userId.Value) != order.UserId) && (!userRole.Value.Equals("Admin")))
            {
                return RedirectToAction("Index", "Order");
            }


            if (order != null)
            {
                if (TempData["OrderSuccess"] != null)
                {
                    ViewBag.OrderSuccess = TempData["OrderSuccess"].ToString();
                }
                return View(order);
            }
            else
            {
                return RedirectToAction("Index", "Order");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(Order order)
        {
            _orderRepository.ChangeOrderStatus(order.Id, 5);
            return RedirectToAction("Detail", "Order", new { id = order.Id });
        }
    }
}
