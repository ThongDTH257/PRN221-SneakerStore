using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repository;
using SneakerStore.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly OrderRepository _orderRepository;

        public AdminOrderController(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            return View("ViewAll");
        }

        public IActionResult ViewAll([FromQuery] int page, [FromQuery] byte status, [FromQuery] string search)
        {
            // Get order list
            int size = 10;
            page = page == 0 ? 1 : page;
            var orderList = _orderRepository.GetAllOrdersByStatusAndIdPagination(page, size, status, search);
            int orderCount = _orderRepository.CountAllOrderByStatusAndId(status, search);
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

            OrderListAdminViewModel model = new()
            {
                OrdersPaginated = orderList,
                Size = size,
                Page = page,
                TotalCount = orderCount,
                TotalPage = totalPages,
                PageNumbers = pageNumbers,
                Status = status,
                Search = search
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
            return View(order);
        }

        [HttpGet]
        public IActionResult Approve(long id)
        {
            Order order = _orderRepository.GetById(id);
            if (order != null)
            {
                if(order.Status == 1)
                {
                    _orderRepository.ChangeOrderStatus(order.Id, 2);
                }
            }
            return RedirectToAction("Detail", new { id = id });
        }

        [HttpGet]
        public IActionResult Reject(long id)
        {
            Order order = _orderRepository.GetById(id);
            if (order != null)
            {
                if (order.Status == 1)
                {
                    _orderRepository.ChangeOrderStatus(order.Id, 3);
                }
            }
            return RedirectToAction("Detail", new { id = id });
        }

        [HttpGet]
        public IActionResult Complete(long id)
        {
            Order order = _orderRepository.GetById(id);
            if (order != null)
            {
                if (order.Status == 2)
                {
                    _orderRepository.ChangeOrderStatus(order.Id, 4);
                }
            }
            return RedirectToAction("Detail", new { id = id });
        }
    }
}
