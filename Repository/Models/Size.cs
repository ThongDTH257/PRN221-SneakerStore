using System;
using System.Collections.Generic;

#nullable disable

namespace Repository.Models
{
    public partial class Size
    {
        public Size()
        {
            CartItems = new HashSet<CartItem>();
            OrderItems = new HashSet<OrderItem>();
            ProductSizes = new HashSet<ProductSize>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ProductSize> ProductSizes { get; set; }
    }
}
