using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Repository.Models;
using Repository.Repository;
using SneakerStore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SneakerStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly BrandRepository _brandRepository;
        private readonly SizeRepository _sizeRepository;

        public ProductController(ProductRepository productRepository, CategoryRepository categoryRepository, BrandRepository brandRepository, SizeRepository sizeRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _sizeRepository = sizeRepository;
        }

        [HttpGet]
        public IActionResult Index([FromQuery] int page, [FromQuery] string search, [FromQuery] long category, [FromQuery] long brand, [FromQuery] long sizeId, [FromQuery] int orderBy)
        {
            // Handle query data
            int size = 12;
            page = page == 0 ? 1 : page;
            var productList = _productRepository.GetAllProductPagination(page, size, search, category, brand, sizeId, orderBy);
            int productCount = _productRepository.CountAllProduct(search, category, brand, sizeId);
            int totalPages = (int)Math.Ceiling((double)productCount / size);
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

            // Get category list for filtering
            var categories = _categoryRepository.GetAll();

            // Get brand list for filtering
            var brands = _brandRepository.GetAll();

            // Get size list for filtering
            var sizes = _sizeRepository.GetAll();

            ProductListClientViewModel model = new()
            {
                ProductsPaginated = productList,
                Size = size,
                Page = page,
                TotalCount = productCount,
                TotalPage = totalPages,
                PageNumbers = pageNumbers,
                Search = search,
                Categories = categories,
                Category = category,
                Brands = brands,
                Brand = brand,
                Sizes = sizes,
                SizeId = sizeId,
                OrderBy = orderBy,
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Detail(long id)
        {
            var product = _productRepository.GetById(id);
            if (product != null)
            {
                var relatedProducts = _productRepository.GetRelatedProduct(id);
                ProductDetailViewModel productDetailViewModel = new ProductDetailViewModel()
                {
                    Product = product,
                    RelatedProducts = relatedProducts,
                    ProductID = id
                };

                if (TempData["AddCartSuccess"] != null)
                {
                    ViewBag.AddCartSuccess = TempData["AddCartSuccess"].ToString();
                }
                return View(productDetailViewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
    }
}
