using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Models;
using Repository.Repository;
using SneakerStore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace SneakerStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;

        public HomeController(UserRepository userRepository, CartRepository cartRepository, ProductRepository productRepository)
        {
            _userRepository = userRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var role = claimsIdentity.FindFirst(ClaimTypes.Role);

            if (role != null && role.Value.Equals("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var mostPopularProduct = _productRepository.GetMostPopularItem();
            // Return Home page
            return View(mostPopularProduct);
        }

        public IActionResult About()
        {
            // Return about page
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Return login page
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel loginModel, string returnUrl)
        {
            ClaimsIdentity identity = null;
            string controller = "Home";

            // Check login account with Database
            User user = _userRepository.Login(loginModel.Email, loginModel.Password);
            if (user == null)
            {
                // User not exists -> back to Login
                ViewBag.ErrorMessage = "Incorrect email or password";
                return View("Views/Home/Login.cshtml");
            }

            if (user.Active == 0)
            {
                // User is inactive
                ViewBag.ErrorMessage = "Your account has been disabled. If you think this was a mistake, try to restore your account by sending us an email.";
                return View("Views/Home/Login.cshtml");
            }

            // Login success:              
            if (user.Admin == 1)
            {
                // Create the identity for the admin  
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, "Admin")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                controller = "Admin";
            }
            else
            {
                // Create the identity for the customer  
                // Get cart item count
                int cartCount = _cartRepository.GetCartCount(user.Id);
                identity = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim(ClaimTypes.UserData, cartCount.ToString()),
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", controller);
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Show register page
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel registerModel)
        {
            // Check confirm password
            if (!registerModel.ConfirmPassword.Equals(registerModel.Password))
            {
                // Back to register if not equal
                ViewBag.ErrorMessage = "Confirm Password does not match";
                return View("Views/Home/Register.cshtml");
            }

            // Check for duplicate email
            if (_userRepository.GetByEmail(registerModel.Email) != null)
            {
                // Back to register if email is used
                ViewBag.ErrorMessage = "This email has already been used";
                return View("Views/Home/Register.cshtml");
            }

            // Proceed to add a new User
            User user = _userRepository.Register(registerModel.Email, registerModel.Phone, registerModel.FullName, registerModel.Password);
            _cartRepository.CreateCart(user.Id);
            ViewBag.SuccessMessage = "Create a new account successfully";
            return View("Views/Home/Register.cshtml");
        }

        [Authorize(Roles = "Admin, Customer")]
        public IActionResult Logout(string returnUrl)
        {
            // Remove Identity
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Back to home
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }
            return Redirect(returnUrl);
        }

        public IActionResult Denied()
        {
            // Return Access denied login page
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Customer")]
        public IActionResult UpdateInformation()
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get user
            User user = _userRepository.GetById(long.Parse(userId.Value));

            if (TempData["UpdateFailMessage"] != null)
            {
                ViewBag.UpdateFailMessage = TempData["UpdateFailMessage"].ToString();
            }

            if (TempData["UpdateSuccessMessage"] != null)
            {
                ViewBag.UpdateSuccessMessage = TempData["UpdateSuccessMessage"].ToString();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Customer")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateInformation(User userModel)
        {
            // Update user in database
            User user = _userRepository.UpdateNameAndPhone(userModel.Id, userModel.Phone, userModel.Name);

            if (user != null)
            {
                // Update user in current context
                // Get current user id  
                var claimsIdentity = User.Identity as ClaimsIdentity;
                ClaimsIdentity identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value),
                    new Claim(ClaimTypes.Email, claimsIdentity.FindFirst(ClaimTypes.Email).Value),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, claimsIdentity.FindFirst(ClaimTypes.Role).Value),
                    new Claim(ClaimTypes.UserData, claimsIdentity.FindFirst(ClaimTypes.UserData).Value),
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Success message
                TempData["UpdateSuccessMessage"] = "Update new information successfully!";
                return RedirectToAction("UpdateInformation");
            }

            TempData["UpdateFailMessage"] = "Unable to update new information";
            return RedirectToAction("UpdateInformation");
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Customer")]
        public IActionResult ChangePassword()
        {
            // Get current user id  
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Logout", "Home");
            }

            // Get user
            User user = _userRepository.GetById(long.Parse(userId.Value));

            ChangePasswordViewModel model = new ChangePasswordViewModel()
            {
                Id = user.Id,
                Email = user.Email,
            };

            if (TempData["UpdateFailMessage"] != null)
            {
                ViewBag.UpdateFailMessage = TempData["UpdateFailMessage"].ToString();
            }

            if (TempData["UpdateSuccessMessage"] != null)
            {
                ViewBag.UpdateSuccessMessage = TempData["UpdateSuccessMessage"].ToString();
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Customer")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // Get user
            User user = _userRepository.GetById(model.Id);

            // Check if current password match
            if(!BC.Verify(model.OldPassword, user.Password))
            {
                TempData["UpdateFailMessage"] = "Your current password is incorrect";
                return RedirectToAction("ChangePassword");
            }

            // Check if new password confirmation
            if (!model.NewPassword.Equals(model.ConfirmPassword))
            {
                TempData["UpdateFailMessage"] = "Confirm password does not match new password";
                return RedirectToAction("ChangePassword");
            }

            // Check if new password equal old password
            if (model.NewPassword.Equals(model.OldPassword))
            {
                TempData["UpdateFailMessage"] = "Your new password cannot be the same as your current password";
                return RedirectToAction("ChangePassword");
            }

            // Update password in database
            user = _userRepository.UpdatePassword(model.Id, model.NewPassword);

            if (user != null)
            {
                // Success message
                TempData["UpdateSuccessMessage"] = "Update password successfully!";
                return RedirectToAction("ChangePassword");
            }

            TempData["UpdateFailMessage"] = "Unable to update new password";
            return RedirectToAction("ChangePassword");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
