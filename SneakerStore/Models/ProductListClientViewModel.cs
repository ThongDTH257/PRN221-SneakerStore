using Repository.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SneakerStore.Models
{
    public class ProductListClientViewModel
    {
        public IQueryable<Product> ProductsPaginated { get; set; }
        public IQueryable<Category> Categories { get; set; }
        public IQueryable<Brand> Brands { get; set; }
        public IQueryable<Size> Sizes { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
        public List<int> PageNumbers { get; set; }
        public string Search { get; set; }
        public long Category { get; set; }
        public long Brand { get; set; }
        public long SizeId { get; set; }
        public int OrderBy { get; set; }

        public ProductListClientViewModel()
        {
            this.Page = 1;
            this.Size = 12;
        }
    }
}
