using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Repository
{
    public class SizeRepository : RepositoryBase<Size>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<Size> _dbSetSize;
        public SizeRepository()
        {
            _context = new SneakerStoreContext();
            _dbSetSize = _context.Set<Size>();
        }
    }
}
