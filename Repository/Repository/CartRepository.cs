using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Linq;

namespace Repository.Repository
{
    public class CartRepository : RepositoryBase<Cart>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Cart> _dbSetCart;
        private readonly DbSet<CartItem> _dbSetCartItem;

        public CartRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetCart = _context.Set<Cart>();
            _dbSetCartItem = _context.Set<CartItem>();
        }

        public Cart CreateCart(long userId)
        {
            Cart cart = new Cart()
            {
                UserId = userId,
            };

            _dbSetCart.Add(cart);
            _context.SaveChanges();
            return cart;
        }

        public int GetCartCount(long userId)
        {
            int count = _dbSetCartItem.Join(_dbSetCart, ci => ci.CartId, c => c.Id, (ci, c) => new
            {
                userId = c.UserId
            }).Where(x => x.userId == userId)
            .Count();

            return count;
        }

        public Cart GetCartByUserId(long userId)
        {
            Cart cart = _dbSetCart.Where(c => c.UserId == userId).FirstOrDefault();

            return cart;
        }
    }
}
