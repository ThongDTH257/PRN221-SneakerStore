using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Linq;

namespace Repository.Repository
{
    public class CategoryRepository : RepositoryBase<Category>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Category> _dbSetCategory;

        public CategoryRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetCategory = _context.Set<Category>();
        }

        public Category GetById(long id)
        {
            return _dbSetCategory.Where(p => p.Id == id).FirstOrDefault();
        }

        public Category CreateCategory(string categoryName)
        {
            Category category = new Category()
            {
                Name = categoryName
            };

            _dbSetCategory.Add(category);
            _context.SaveChanges();
            return category;
        }

        public void UpdateCategory(long id, string categoryName)
        {
            Category category = GetById(id);
            if (category != null)
            {
                category.Name = categoryName;
            }

            _context.Attach(category);
            _context.Entry(category).Property(p => p.Name).IsModified = true;
            _context.SaveChanges();
        }
    }
}
