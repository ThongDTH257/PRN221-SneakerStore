using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Repository.Repository
{
    public class CartItemRepository : RepositoryBase<CartItem>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<CartItem> _dbSet;
        public CartItemRepository()
        {
            _context = new SneakerStoreContext();
            _dbSet = _context.Set<CartItem>();
        }

        public CartItem GetById(long cartId, long productId, long sizeId)
        {
            CartItem cartItem = _dbSet
                .Where(ci => ci.CartId == cartId && ci.ProductId == productId && ci.SizeId == sizeId && ci.Product.Active == 1)
                .FirstOrDefault();
            return cartItem;
        }

        public IQueryable<CartItem> GetByCartId(long cartId)
        {
            var cartItems = _dbSet
                .Where(ci => ci.CartId == cartId && ci.Product.Active == 1)
                .Include(ci => ci.Size)
                .Include(ci => ci.Product)
                .ThenInclude(p => p.Brand)
                .AsQueryable();
            return cartItems;
        }

        public CartItem AddToCart(long cartId, long productId, long sizeId, int quantity)
        {
            // Check if cart item already exist?
            CartItem cartItem = GetById(cartId, productId, sizeId);
            if(cartItem == null ) 
            {
                // Add new cart item
                cartItem = new CartItem()
                {
                    CartId= cartId,
                    ProductId= productId,
                    SizeId= sizeId,
                    Quantity= quantity
                };
                _dbSet.Add(cartItem);
                _context.SaveChanges();
                return cartItem;
            }

            // Update quantity
            cartItem.Quantity += quantity;
            _context.Attach(cartItem);
            _context.Entry(cartItem).Property(ci => ci.Quantity).IsModified = true;
            _context.SaveChanges();

            return cartItem;
        }

        public void RemoveCartItem(long cartId, long productId, long sizeId)
        {
            CartItem cartItem = GetById(cartId, productId, sizeId);
            if(cartItem != null)
            {
                _dbSet.Remove(cartItem);
                _context.SaveChanges();
            }
        }

        public void DeleteCart(long cartId)
        {
            List<CartItem> cartItems = GetByCartId(cartId).ToList();
            if (cartItems.Count() > 0)
            {
                _dbSet.RemoveRange(cartItems);
                _context.SaveChanges();
            }
        }

        public void UpdateCartItem(long cartId, long productId, long sizeId, int quantity)
        {
            CartItem cartItem = GetById(cartId, productId, sizeId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.Attach(cartItem);
                _context.Entry(cartItem).Property(ci => ci.Quantity).IsModified = true;
                _context.SaveChanges();
            }
        }
    }
}
