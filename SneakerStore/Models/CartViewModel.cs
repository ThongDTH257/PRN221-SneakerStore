using Repository.Models;
using System.Linq;

namespace SneakerStore.Models
{
    public class CartViewModel
    {
        public IQueryable<CartItem> CartItems { get; set; }
        public int TotalItems { get; set; }
        public int TotalQuantities { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
