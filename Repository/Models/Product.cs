using System;
using System.Collections.Generic;

#nullable disable

namespace Repository.Models
{
    public partial class Product
    {
        public Product()
        {
            CartItems = new HashSet<CartItem>();
            OrderItems = new HashSet<OrderItem>();
            ProductSizes = new HashSet<ProductSize>();
            ProductTags = new HashSet<ProductTag>();
        }

        public long Id { get; set; }
        public byte Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? BrandId { get; set; }
        public long? CategoryId { get; set; }
        public long? CreatorId { get; set; }

        public virtual Brand Brand { get; set; }
        public virtual Category Category { get; set; }
        public virtual User Creator { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ProductSize> ProductSizes { get; set; }
        public virtual ICollection<ProductTag> ProductTags { get; set; }
    }
}
