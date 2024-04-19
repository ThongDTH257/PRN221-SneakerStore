using System;
using System.Collections.Generic;

#nullable disable

namespace Repository.Models
{
    public partial class CartItem
    {
        public int? Quantity { get; set; }
        public long ProductId { get; set; }
        public long SizeId { get; set; }
        public long CartId { get; set; }

        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
        public virtual Size Size { get; set; }
    }
}
