using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Linq;

public class RepositoryBase<T> where T : class
{
    private readonly SneakerStoreContext _context;
    private readonly DbSet<T> _dbSet;
    public RepositoryBase()
    {
        _context = new SneakerStoreContext();
        _dbSet = _context.Set<T>();
    }
    public IQueryable<T> GetAll()
    {
        return _dbSet.AsQueryable();
    }

    public void Create(T entity)
    {
        _dbSet.Add(entity);
        _context.SaveChanges();
    }
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }
    public void Update(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
        //_dbSet.Update(entity);
        _context.SaveChanges();
    }
}