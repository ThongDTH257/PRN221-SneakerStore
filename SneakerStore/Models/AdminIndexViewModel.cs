using Repository.Models;
using System.Linq;

namespace SneakerStore.Models
{
    public class AdminIndexViewModel
    {
        public int PendingOrderCount { get; set; }
        public int TotalOrderCount { get; set; }
        public int TotalProductCount { get; set; }
        public int TotalCustomerCount { get; set; }
        public IQueryable<Product> BestProducts { get; set; }
        public IQueryable<Order> RecentOrders { get; set; }

    }
}
