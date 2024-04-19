using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SneakerStore.Models
{
    public class OrderListAdminViewModel
    {
        public IQueryable<Order> OrdersPaginated { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
        public List<int> PageNumbers { get; set; }
        public byte Status { get; set; }
        public string Search { get; set; }

        public OrderListAdminViewModel()
        {
            this.Page = 1;
            this.Size = 10;
            this.Status = 0;
        }
    }
}
