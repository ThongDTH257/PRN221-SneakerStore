using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Linq;

namespace Repository.Repository
{
    public class BrandRepository : RepositoryBase<Brand>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Brand> _dbSetBrand;
        public BrandRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetBrand = _context.Set<Brand>();
        }

        public Brand GetById(long id)
        {
            return _dbSetBrand.Where(p => p.Id == id).FirstOrDefault();
        }

        public Brand CreateBrand(string brandName)
        {
            Brand brand = new Brand()
            {
                Name = brandName
            };

            _dbSetBrand.Add(brand);
            _context.SaveChanges();
            return brand;
        }

        public void UpdateBrand(long id, string brandName)
        {
            Brand brand = GetById(id);
            if(brand != null)
            {
                brand.Name = brandName;
            }

            _context.Attach(brand);
            _context.Entry(brand).Property(p => p.Name).IsModified=true;
            _context.SaveChanges();
        }
    }
}
