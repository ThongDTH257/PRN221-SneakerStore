using Microsoft.AspNetCore.Http;
using Repository.Models;
using System.Collections.Generic;
using System.Linq;

namespace SneakerStore.Models
{
    public class ProductUpdateViewModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public long? BrandId { get; set; }
        public long? CategoryId { get; set; }
        public List<long> Sizes { get; set; }
        public IQueryable<Brand> BrandList { get; set; }
        public IQueryable<Category> CategoryList { get; set; }
        public IQueryable<Size> SizeList { get; set; }
        public IFormFile UploadImage { get; set; }
    }
}
