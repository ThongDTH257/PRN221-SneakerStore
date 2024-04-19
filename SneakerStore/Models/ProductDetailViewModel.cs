using Repository.Models;
using System.Linq;

namespace SneakerStore.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public IQueryable<Product> RelatedProducts { get; set; }
        public long ProductID { get; set; }
        public long SizeID { get; set; }
        public int Quantity { get; set; }
    }
}
