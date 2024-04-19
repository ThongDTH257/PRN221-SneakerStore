using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Linq;

namespace Repository.Repository
{
    public class TagRepository : RepositoryBase<Tag>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Tag> _dbSetTag;
        public TagRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetTag = _context.Set<Tag>();
        }

        public Tag GetById(long id)
        {
            return _dbSetTag.Where(p => p.Id == id).FirstOrDefault();
        }

        public Tag CreateTag(string tagName)
        {
            Tag tag = new Tag()
            {
                Name = tagName
            };

            _dbSetTag.Add(tag);
            _context.SaveChanges();
            return tag;
        }

        public void UpdateTag(long id, string tagName)
        {
            Tag tag = GetById(id);
            if (tag != null)
            {
                tag.Name = tagName;
            }

            _context.Attach(tag);
            _context.Entry(tag).Property(p => p.Name).IsModified = true;
            _context.SaveChanges();
        }
    }
}
