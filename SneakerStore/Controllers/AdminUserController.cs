using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repository;
using SneakerStore.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserRepository _userRepository;

        public AdminUserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View("ViewAll");
        }

        [HttpGet]
        public IActionResult ViewAll([FromQuery] int page, [FromQuery] string search)
        {
            // Handle query data
            int size = 10;
            page = page == 0 ? 1 : page;
            var userList = _userRepository.GetAllUserPagination(page, size, search);
            int userCount = _userRepository.CountAllUserPagination(search);
            int totalPages = (int)Math.Ceiling((double)userCount / size);
            List<int> pageNumbers = new List<int>();
            if (totalPages > 0)
            {
                int start = Math.Max(1, page - 2);
                int end = Math.Min(page + 2, totalPages);

                if (totalPages > 5)
                {
                    if (end == totalPages) start = end - 4;
                    else if (start == 1) end = start + 4;
                }
                else
                {
                    start = 1;
                    end = totalPages;
                }
                pageNumbers = Enumerable.Range(start, end - start + 1).ToList();
            }

            UserUpdateViewModel users = new UserUpdateViewModel()
            {
                UserList = userList,
                Size = size,
                Page = page,
                TotalCount = userCount,
                TotalPage = totalPages,
                PageNumbers = pageNumbers,
                Search = search,
            };
            if (TempData["ChangeStatusSuccess"] != null)
            {
                ViewBag.ChangeStatusSuccess = TempData["ChangeStatusSuccess"].ToString();
            }
            return View(users);
        }
        
        [HttpGet]
        public IActionResult Update(long id)
        {
            User user = _userRepository.GetById(id);
            if (user != null)
            {
                _userRepository.ChangeUserStatus(id);
            }
            TempData["ChangeStatusSuccess"] = "Update User successfully!";
            return RedirectToAction("ViewAll");
        }
    }
}
