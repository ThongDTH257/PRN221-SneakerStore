using System;
using System.Collections.Generic;

#nullable disable

namespace Repository.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public long Id { get; set; }
        public string Address { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Payment { get; set; }
        public string Phone { get; set; }
        public byte Status { get; set; }
        public int? TotalItem { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
