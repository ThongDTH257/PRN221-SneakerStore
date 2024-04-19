using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.Repository;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBrandController : Controller
    {
        private readonly BrandRepository _brandRepository;

        public AdminBrandController (BrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
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
        public IActionResult Create(Brand brand)
        {
            Brand newBrand = _brandRepository.CreateBrand(brand.Name);
            TempData["CreateBrandSuccess"] = "Add a Brand successfully!";
            return RedirectToAction("ViewAll");
        }

        [HttpGet]
        public IActionResult ViewAll()
        {
            var brandList = _brandRepository.GetAll();
            if (TempData["CreateBrandSuccess"] != null)
            {
                ViewBag.CreateBrandSuccess = TempData["CreateBrandSuccess"].ToString();
            }
            return View(brandList);
        }

        [HttpGet]
        public IActionResult Update(long id)
        {
            Brand brand = _brandRepository.GetById(id);
            return View(brand);
        }

        [HttpPost]
        public IActionResult Update(Brand brand)
        {
            _brandRepository.UpdateBrand(brand.Id, brand.Name);
            TempData["CreateBrandSuccess"] = "Update a Brand successfully!";
            return RedirectToAction("ViewAll");
        }
    }
}
