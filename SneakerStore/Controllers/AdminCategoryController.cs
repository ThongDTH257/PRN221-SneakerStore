using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repository;
using System.Linq;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;

        public AdminCategoryController(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            return View("ViewAll");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            _categoryRepository.CreateCategory(category.Name);
            TempData["CreateCategorySuccess"] = "Add a Category successfully!";
            return RedirectToAction("ViewAll");
        }

        [HttpGet]
        public IActionResult ViewAll()
        {
            var categoryList = _categoryRepository.GetAll().OrderBy(c => c.Id);
            if (TempData["CreateCategorySuccess"] != null)
            {
                ViewBag.CreateCategorySuccess = TempData["CreateCategorySuccess"].ToString();
            }
            return View(categoryList);
        }

        [HttpGet]
        public IActionResult Update(long id) 
        {
            Category category = _categoryRepository.GetById(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Update(Category category)
        {
            _categoryRepository.UpdateCategory(category.Id, category.Name);
            TempData["CreateCategorySuccess"] = "Update a Category successfully!";
            return RedirectToAction("ViewAll");
        }
    }
}
