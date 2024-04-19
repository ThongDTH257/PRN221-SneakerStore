using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Repository.Repository
{
    public class ProductRepository : RepositoryBase<Product>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Product> _dbSetProduct;
        private readonly DbSet<OrderItem> _dbSetOrderItem;
        ProductSizeRepository _productSizeRepository;
        public ProductRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetProduct = _context.Set<Product>();
            _productSizeRepository = new ProductSizeRepository();
            _dbSetOrderItem = _context.Set<OrderItem>();
        }

        public IQueryable<Product> GetAllProduct()
        {
            return _dbSetProduct
                .Where(p => p.Active == 1)
                .Include(p => p.Brand)
                .AsQueryable();
        }

        public List<long> GetMostPopularItemIds()
        {
            return _dbSetOrderItem
                .GroupBy(x => x.ProductId)
                .Select(x => new { ProductId = x.Key, QuantitySum = x.Sum(a => a.Quantity) })
                .OrderByDescending(x => x.QuantitySum)
                .Select(x => x.ProductId)
                .Take(3)
                .AsQueryable()
                .ToList();
        }

        public IQueryable<Product> GetMostPopularItem()
        {
            List<long> productIds = GetMostPopularItemIds();
            return _dbSetProduct
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();
        }

        public IQueryable<Product> GetAllProductPagination(int page, int size, string search, long categoryId, long brandId, long sizeId, int orderBy)
        {
            var products = _dbSetProduct
                .Where(p => p.Active == 1
                && (string.IsNullOrEmpty(search) || p.Name.Contains(search))
                && (categoryId == 0 || p.CategoryId == categoryId)
                && (brandId == 0 || p.BrandId == brandId)
                && (sizeId == 0 || p.ProductSizes.Any(ps => ps.SizeId == sizeId)));
            switch (orderBy)
            {
                case 1:
                    products = products.OrderBy(p => p.CreatedAt);
                    break;
                case 2:
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case 3:
                    products = products.OrderBy(p => p.Price);
                    break;
                default:
                    products = products.OrderByDescending(p => p.CreatedAt);
                    break;
            }


            return products
                .Skip((page - 1) * size)
                .Take(size)
                .Include(p => p.Brand)
                .AsQueryable();
        }

        public int CountAllProduct(string search, long categoryId, long brandId, long sizeId)
        {
            return _dbSetProduct
                .Where(p => p.Active == 1
                && (string.IsNullOrEmpty(search) || p.Name.Contains(search))
                && (categoryId == 0 || p.CategoryId == categoryId)
                && (brandId == 0 || p.BrandId == brandId)
                && (sizeId == 0 || p.ProductSizes.Any(ps => ps.SizeId == sizeId)))
                .Include(p => p.Brand)
                .Count();
        }

        public IQueryable<Product> GetAllProductPaginationAdmin(int page, int size, string search, long categoryId, long brandId, long sizeId, int orderBy)
        {
            var products = _dbSetProduct
                .Where(p => (string.IsNullOrEmpty(search) || p.Name.Contains(search))
                && (categoryId == 0 || p.CategoryId == categoryId)
                && (brandId == 0 || p.BrandId == brandId)
                && (sizeId == 0 || p.ProductSizes.Any(ps => ps.SizeId == sizeId)));
            switch (orderBy)
            {
                case 1:
                    products = products.OrderBy(p => p.CreatedAt);
                    break;
                case 2:
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case 3:
                    products = products.OrderBy(p => p.Price);
                    break;
                default:
                    products = products.OrderByDescending(p => p.CreatedAt);
                    break;
            }


            return products
                .Skip((page - 1) * size)
                .Take(size)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();
        }

        public int CountAllProductAdmin(string search, long categoryId, long brandId, long sizeId)
        {
            return _dbSetProduct
                .Where(p => (string.IsNullOrEmpty(search) || p.Name.Contains(search))
                && (categoryId == 0 || p.CategoryId == categoryId)
                && (brandId == 0 || p.BrandId == brandId)
                && (sizeId == 0 || p.ProductSizes.Any(ps => ps.SizeId == sizeId)))
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Count();
        }

        public IQueryable<Product> GetRelatedProduct(long productId)
        {
            Product product = GetById(productId);
            if (product == null)
            {
                return Enumerable.Empty<Product>().AsQueryable();
            }
            return _dbSetProduct
                .Where(p => (p.BrandId == product.BrandId || p.CategoryId == product.CategoryId) && p.Id != product.Id && p.Active == 1)
                .Include(p => p.Brand).Distinct().Take(4).AsQueryable();
        }

        public Product GetById(long id)
        {
            return _dbSetProduct.Where(p => p.Id == id && p.Active == 1)
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.ProductSizes).ThenInclude(ps => ps.Size)
                .FirstOrDefault();
        }

        public Product GetByIdAllStatus(long id)
        {
            return _dbSetProduct.Where(p => p.Id == id)
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.ProductSizes).ThenInclude(ps => ps.Size)
                .FirstOrDefault();
        }

        public Product CreateProduct(string description, string name, string image, decimal price, 
            long? brandId, long? categoryId, List<long> sizes)
        {
            Product product = new Product()
            {
                Active = 1,
                Description = description,
                Name = name,
                Image = image,
                Price = price,
                BrandId = brandId,
                CategoryId = categoryId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatorId = 1
            };

            _dbSetProduct.Add(product);
            _context.SaveChanges();

            long productId = product.Id;
            
            foreach (long size in sizes)
            {
                _productSizeRepository.AddSize(size, productId);
            }
            return product;
        }

        public Product UpdateProduct(long id, string description, string name, string image, decimal price,
            long? brandId, long? categoryId, List<long> sizes)
        {
            Product product = GetById(id);
            if (product != null)
            {
                product.Description = description;
                product.Name = name;
                product.Image = image;
                product.Price = price;
                product.BrandId = brandId;
                product.CategoryId = categoryId;
                product.UpdatedAt = DateTime.Now;
            }
            _dbSetProduct.Update(product);
            _productSizeRepository.RemoveAllSizeOfProduct(product.Id);

            foreach (long size in sizes)
            {
                _productSizeRepository.AddSize(size, product.Id);
            }
            _context.SaveChanges();
            return product;
        }

        public void ChangeStatus(long id)
        {
            Product product = GetByIdAllStatus(id);
            if (product != null)
            {
                if (product.Active == 0)
                {
                    product.Active = 1;
                }
                else
                {
                    product.Active = 0;
                }
            }
            _dbSetProduct.Update(product);
            _context.SaveChanges();
        }
    }
}
