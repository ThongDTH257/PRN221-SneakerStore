using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Repository;
using SneakerStore.Models;
using System.Data;

namespace SneakerStore.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;

        public AdminController(ProductRepository productRepository, UserRepository userRepository, OrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            int pendingOrderCount = _orderRepository.CountAllOrderByStatus(1);
            int totalOrderCount = _orderRepository.CountAllOrderByStatus(0);
            int totalProductCount = _productRepository.CountAllProductAdmin("", 0, 0, 0);
            int totalCustomerCount = _userRepository.CountAllCustomerPagination();

            var bestProducts = _productRepository.GetMostPopularItem();
            var recentOrders = _orderRepository.GetRecentOrder();

            AdminIndexViewModel model = new AdminIndexViewModel()
            {
                PendingOrderCount = pendingOrderCount,
                TotalOrderCount = totalOrderCount,
                TotalProductCount = totalProductCount,
                TotalCustomerCount = totalCustomerCount,
                BestProducts = bestProducts,
                RecentOrders = recentOrders
            };

            return View(model);
        }
    }
}
