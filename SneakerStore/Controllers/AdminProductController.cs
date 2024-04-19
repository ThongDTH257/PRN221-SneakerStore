using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Models;
using Repository.Repository;
using SneakerStore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firebase.Auth;
using System.Threading.Tasks;
using System.Threading;
using Firebase.Storage;

namespace SneakerStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ProductRepository _productRepository;
        private readonly BrandRepository _brandRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly SizeRepository _sizeRepository;
        private readonly ProductSizeRepository _productSizeRepository;

        public AdminProductController(ProductRepository productRepository,
            BrandRepository brandRepository, CategoryRepository categoryRepository,
            SizeRepository sizeRepository, ProductSizeRepository productSizeRepository, IWebHostEnvironment env)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _sizeRepository = sizeRepository;
            _productSizeRepository = productSizeRepository;
            _env = env;
        }

        // Configure firebase
        private static string ApiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("firebaseConfig")["apiKey"];
        private static string Bucket = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("firebaseConfig")["storageBucket"];
        private static string AuthEmail = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("firebaseConfig")["authEmail"];
        private static string AuthPassword = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("firebaseConfig")["authPassword"];

        public IActionResult Index()
        {
            return RedirectToAction("ViewAll");
        }

        [HttpGet]
        public IActionResult Create()
        {
            var brandList = _brandRepository.GetAll();
            var categoryList = _categoryRepository.GetAll();
            var sizeList = _sizeRepository.GetAll();

            ProductCreateViewModel createViewModel = new ProductCreateViewModel
            {
                BrandList = brandList,
                CategoryList = categoryList,
                SizeList = sizeList
            };

            if (TempData["SizeEmptyMessage"] != null)
            {
                ViewBag.SizeEmptyMessage = TempData["SizeEmptyMessage"].ToString();
            }

            return View(createViewModel);
        }

        [HttpGet]
        public IActionResult Detail(long id)
        {
            var product = _productRepository.GetByIdAllStatus(id);
            if (product != null)
            {
                return View(product);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel product)
        {
            // Get Img file
            var fileUpload = product.UploadImage;
            FileStream fs;

            if (fileUpload != null && fileUpload.Length > 0)
            {
                string fileExtension = Path.GetExtension(fileUpload.FileName).Substring(1);
                string fileName = $"{Path.GetRandomFileName()}.{fileExtension}";
                // Upload file to firebase
                string folderName = "product-upload";
                string path = Path.Combine(_env.WebRootPath, $"images/{folderName}");
                using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await fileUpload.CopyToAsync(fs);
                }
                fs = new FileStream(Path.Combine(path, fileName), FileMode.Open);
                // Firebase uploading
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                // Cancellation Token
                var upload = new FirebaseStorage
                (
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                )
                .Child(fileName)
                .PutAsync(fs);

                var downloadUrl = await upload;

                product.Image = downloadUrl;

            }

            if (product.Sizes?.Any() != true)
            {
                TempData["SizeEmptyMessage"] = "Must pick at least a size for this product";
                return RedirectToAction("Create");
            }
            Product newProduct = _productRepository.CreateProduct(product.Description,
                product.Name, product.Image, product.Price, product.BrandId, product.CategoryId, product.Sizes);


            TempData["CreateProductSuccess"] = "Add a Product successfully!";
            return RedirectToAction("ViewAll");
        }

        [HttpGet]
        public IActionResult ViewAll([FromQuery] int page, [FromQuery] string search, [FromQuery] long category, [FromQuery] long brand, [FromQuery] long sizeId, [FromQuery] int orderBy)
        {
            // Handle query data
            int size = 12;
            page = page == 0 ? 1 : page;
            var productList = _productRepository.GetAllProductPaginationAdmin(page, size, search, category, brand, sizeId, orderBy);
            int productCount = _productRepository.CountAllProductAdmin(search, category, brand, sizeId);
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

            if (TempData["CreateProductSuccess"] != null)
            {
                ViewBag.CreateProductSuccess = TempData["CreateProductSuccess"].ToString();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Update(long id)
        {
            if (id > 0)
            {
                var product = _productRepository.GetById(id);
                var brandList = _brandRepository.GetAll();
                var categoryList = _categoryRepository.GetAll();
                var sizeList = _sizeRepository.GetAll();

                ProductUpdateViewModel updateViewModel = new ProductUpdateViewModel
                {
                    Id = id,
                    Description = product.Description,
                    Name = product.Name,
                    Image = product.Image,
                    Price = product.Price,
                    BrandId = product.BrandId,
                    CategoryId = product.CategoryId,
                    BrandList = brandList,
                    CategoryList = categoryList,
                    SizeList = sizeList,
                    Sizes = product.ProductSizes.Select(p => p.SizeId).ToList()
                };

                if (TempData["SizeEmptyMessage"] != null)
                {
                    ViewBag.SizeEmptyMessage = TempData["SizeEmptyMessage"].ToString();
                }

                return View(updateViewModel);
            }
            return RedirectToAction("ViewAll");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProductUpdateViewModel product)
        {
            // Get Img file
            var fileUpload = product.UploadImage;
            FileStream fs;

            if (fileUpload != null && fileUpload.Length > 0)
            {
                string fileExtension = Path.GetExtension(fileUpload.FileName).Substring(1);
                string fileName = $"{Path.GetRandomFileName()}.{fileExtension}";
                // Upload file to firebase
                string folderName = "product-upload";
                string path = Path.Combine(_env.WebRootPath, $"images/{folderName}");
                using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await fileUpload.CopyToAsync(fs);
                }
                fs = new FileStream(Path.Combine(path, fileName), FileMode.Open);
                // Firebase uploading
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                // Cancellation Token
                var upload = new FirebaseStorage
                (
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                )
                .Child(fileName)
                .PutAsync(fs);

                var downloadUrl = await upload;

                product.Image = downloadUrl;
            }


            if (product.Sizes?.Any() != true)
            {
                TempData["SizeEmptyMessage"] = "Must pick at least a size for this product";
                return RedirectToAction("Update", new { id = product.Id });
            }
            _productRepository.UpdateProduct(product.Id, product.Description,
                product.Name, product.Image, product.Price, product.BrandId, product.CategoryId, product.Sizes);
            TempData["CreateProductSuccess"] = "Update Product successfully!";
            return RedirectToAction("ViewAll");
        }

        public IActionResult ChangeStatus(long id)
        {
            _productRepository.ChangeStatus(id);
            TempData["CreateProductSuccess"] = "Update Product successfully!";
            return RedirectToAction("ViewAll");
        }
    }
}
