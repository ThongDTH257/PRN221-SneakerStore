using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repository;
using SneakerStore.Models;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Customer, Admin")]
    public class CartController : Controller
    {
        private readonly CartItemRepository _cartItemRepository;
        private readonly CartRepository _cartRepository;
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;

        public CartController(CartRepository cartRepository, CartItemRepository cartItemRepository, UserRepository userRepository, OrderRepository orderRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Cart page
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get Cart from User
            Cart cart = _cartRepository.GetCartByUserId(long.Parse(userId.Value));

            // Get Cart itesm
            IQueryable<CartItem> cartItems = _cartItemRepository.GetByCartId(cart.Id);
            CartViewModel cartViewModel = new CartViewModel()
            {
                CartItems = cartItems.OrderBy(ci => ci.ProductId),
                TotalItems = cartItems.Count(),
                TotalQuantities = cartItems.Sum(ci => ci.Quantity).Value,
                TotalPrice = cartItems.Sum(ci => ci.Quantity * ci.Product.Price).Value
            };

            return View(cartViewModel);
        }

        [HttpGet]
        public IActionResult Add(long id)
        {
            return RedirectToAction("Detail", "Product", new {id = id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(ProductDetailViewModel addToCartModel)
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get Cart from User
            Cart cart = _cartRepository.GetCartByUserId(long.Parse(userId.Value));

            // Add new cart item or update quantity or current item
            CartItem cartItem = _cartItemRepository.AddToCart(cart.Id, addToCartModel.ProductID, addToCartModel.SizeID, addToCartModel.Quantity);

            // Update cart item count
            int cartCount = _cartRepository.GetCartCount(long.Parse(userId.Value));

            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value),
                new Claim(ClaimTypes.Email, claimsIdentity.FindFirst(ClaimTypes.Email).Value),
                new Claim(ClaimTypes.Name, claimsIdentity.FindFirst(ClaimTypes.Name).Value),
                new Claim(ClaimTypes.Role, claimsIdentity.FindFirst(ClaimTypes.Role).Value),
                new Claim(ClaimTypes.UserData, cartCount.ToString()),
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["AddCartSuccess"] = "Add to cart successfully";
            return RedirectToAction("Detail", "Product", new { id = addToCartModel.ProductID });
        }

        [HttpGet]
        public IActionResult Remove([FromQuery] long productId, [FromQuery] long sizeId)
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get Cart from User
            Cart cart = _cartRepository.GetCartByUserId(long.Parse(userId.Value));

            // Remove cartitem by cartid, prodid, sizeid
            _cartItemRepository.RemoveCartItem(cart.Id, productId, sizeId);

            // Update cart item count
            int cartCount = _cartRepository.GetCartCount(long.Parse(userId.Value));

            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value),
                new Claim(ClaimTypes.Email, claimsIdentity.FindFirst(ClaimTypes.Email).Value),
                new Claim(ClaimTypes.Name, claimsIdentity.FindFirst(ClaimTypes.Name).Value),
                new Claim(ClaimTypes.Role, claimsIdentity.FindFirst(ClaimTypes.Role).Value),
                new Claim(ClaimTypes.UserData, cartCount.ToString()),
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Change([FromQuery] long productId, [FromQuery] long sizeId, [FromQuery] int newQuantity)
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get Cart from User
            Cart cart = _cartRepository.GetCartByUserId(long.Parse(userId.Value));

            // Remove cartitem by cartid, prodid, sizeid
             _cartItemRepository.UpdateCartItem(cart.Id, productId, sizeId, newQuantity);

            // Update cart item count
            int cartCount = _cartRepository.GetCartCount(long.Parse(userId.Value));

            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value),
                new Claim(ClaimTypes.Email, claimsIdentity.FindFirst(ClaimTypes.Email).Value),
                new Claim(ClaimTypes.Name, claimsIdentity.FindFirst(ClaimTypes.Name).Value),
                new Claim(ClaimTypes.Role, claimsIdentity.FindFirst(ClaimTypes.Role).Value),
                new Claim(ClaimTypes.UserData, cartCount.ToString()),
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get current user email and phone
            User user = _userRepository.GetById(long.Parse(userId.Value));
            string email = user.Email;
            string phone = user.Phone;

            // Get Cart from User
            Cart cart = _cartRepository.GetCartByUserId(long.Parse(userId.Value));

            // Get Cart itesm
            IQueryable<CartItem> cartItems = _cartItemRepository.GetByCartId(cart.Id);

            CheckoutViewModel model = new()
            {
                Email = email,
                Phone = phone,
                TotalPrice = cartItems.Sum(ci => ci.Quantity * ci.Product.Price).Value
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get Cart from User
            Cart cart = _cartRepository.GetCartByUserId(long.Parse(userId.Value));

            // Get Cart items
            IQueryable<CartItem> cartItems = _cartItemRepository.GetByCartId(cart.Id);

            // Create order and add order items
            Order order = _orderRepository.CreateOrder(long.Parse(userId.Value), model.Phone, model.Address, model.PaymentMethod, model.TotalPrice, cartItems);

            // Delete cart items
            _cartItemRepository.DeleteCart(cart.Id);

            // Update cart item count
            int cartCount = _cartRepository.GetCartCount(long.Parse(userId.Value));

            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value),
                new Claim(ClaimTypes.Email, claimsIdentity.FindFirst(ClaimTypes.Email).Value),
                new Claim(ClaimTypes.Name, claimsIdentity.FindFirst(ClaimTypes.Name).Value),
                new Claim(ClaimTypes.Role, claimsIdentity.FindFirst(ClaimTypes.Role).Value),
                new Claim(ClaimTypes.UserData, cartCount.ToString()),
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["OrderSuccess"] = "Order Successfully!";
            return RedirectToAction("Detail","Order", new { id = order.Id});
        }
    }
}
