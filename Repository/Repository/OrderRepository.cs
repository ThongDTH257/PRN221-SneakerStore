using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repository
{
    public class OrderRepository : RepositoryBase<Order>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Order> _dbSetOrder;
        private readonly DbSet<OrderItem> _dbSetOrderItem;

        public OrderRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetOrder = _context.Set<Order>();
            _dbSetOrderItem = _context.Set<OrderItem>();
        }

        public Order CreateOrder(long userId, string phone, string address, string payment, decimal totalPrice, IQueryable<CartItem> cartItems)
        {
            // Add order
            Order order = new Order()
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Phone = phone,
                Address = address,
                Payment = payment,
                Status = 1,
                TotalItem = 0,
                TotalPrice = 0,
            };
            _dbSetOrder.Add(order);
            _context.SaveChanges();

            // Add order items
            List<OrderItem> orderItems = new List<OrderItem>();
            foreach (CartItem cartItem in cartItems)
            {
                orderItems.Add(new OrderItem()
                {
                    OrderId = order.Id,
                    SizeId = cartItem.SizeId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                });
            }

            _dbSetOrderItem.AddRange(orderItems);
            _context.SaveChanges();

            // Update order price and item
            order.TotalItem = orderItems.Count;
            order.TotalPrice = totalPrice;
            _context.Attach(order);
            _context.Entry(order).Property(oi => oi.TotalItem).IsModified = true;
            _context.Entry(order).Property(oi => oi.TotalPrice).IsModified = true;
            _context.SaveChanges();

            return order;
        }

        public Order GetById(long orderId)
        {
            return _dbSetOrder
                .Where(o => o.Id == orderId)
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Size)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Brand)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Category)
                .FirstOrDefault();
        }

        public IQueryable<Order> GetRecentOrder()
        {
            return _dbSetOrder
                .OrderByDescending(o => o.CreatedAt)
                .Take(4)
                .AsQueryable();
        }

        public IQueryable<Order> GetOrdersByUserId(long userId)
        {
            return _dbSetOrder
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();
        }

        public IQueryable<Order> GetOrdersByUserIdAndStatusPagination(long userId, int page, int size, byte status)
        {
            if (status == 0)
            {
                return _dbSetOrder
                                .Where(o => o.UserId == userId)
                                .OrderByDescending(o => o.CreatedAt)
                                .Skip((page - 1) * size)
                                .Take(size)
                                .AsQueryable();
            }
            return _dbSetOrder
                .Where(o => o.UserId == userId && o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .AsQueryable();
        }

        public int CountAllOrderByUserIdAndStatus(long userId, byte status)
        {
            if (status == 0)
            {
                return _dbSetOrder
                                .Where(o => o.UserId == userId)
                                .Count();
            }
            return _dbSetOrder
              .Where(o => o.UserId == userId && o.Status == status)
              .Count();
        }

        public IQueryable<Order> GetAllOrdersByStatusPagination(int page, int size, byte status)
        {
            if (status == 0)
            {
                return _dbSetOrder
                                .OrderByDescending(o => o.CreatedAt)
                                .Skip((page - 1) * size)
                                .Take(size)
                                .Include(o => o.User)
                                .AsQueryable();
            }
            return _dbSetOrder
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Include(o => o.User)
                .AsQueryable();
        }

        public int CountAllOrderByStatus(byte status)
        {
            if (status == 0)
            {
                return _dbSetOrder
                                .Count();
            }
            return _dbSetOrder
              .Where(o => o.Status == status)
              .Count();
        }

        public IQueryable<Order> GetAllOrdersByStatusAndIdPagination(int page, int size, byte status, string search)
        {
            if (status == 0)
            {
                return _dbSetOrder
                    .Where(o => (string.IsNullOrEmpty(search) || o.Id.ToString().Contains(search)))
                                .OrderByDescending(o => o.CreatedAt)
                                .Skip((page - 1) * size)
                                .Take(size)
                                .Include(o => o.User)
                                .AsQueryable();
            }
            return _dbSetOrder
                .Where(o => o.Status == status
                && (string.IsNullOrEmpty(search) || o.Id.ToString().Contains(search)))
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Include(o => o.User)
                .AsQueryable();
        }

        public int CountAllOrderByStatusAndId(byte status, string search)
        {
            if (status == 0)
            {
                return _dbSetOrder
                    .Where(o => (string.IsNullOrEmpty(search) || o.Id.ToString().Contains(search)))
                                .Count();
            }
            return _dbSetOrder
              .Where(o => o.Status == status
              && (string.IsNullOrEmpty(search) || o.Id.ToString().Contains(search)))
              .Count();
        }

        public void ChangeOrderStatus(long id, byte status)
        {
            Order order = GetById(id);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.Now;
            }
            _dbSetOrder.Update(order);
            _context.SaveChanges();
        }
    }
}
