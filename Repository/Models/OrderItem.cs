using System;
using System.Collections.Generic;

#nullable disable

namespace Repository.Models
{
    public partial class OrderItem
    {
        public int? Quantity { get; set; }
        public long ProductId { get; set; }
        public long OrderId { get; set; }
        public long SizeId { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual Size Size { get; set; }
    }
}
