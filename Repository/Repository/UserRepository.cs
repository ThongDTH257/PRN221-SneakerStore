using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Linq;
using BC = BCrypt.Net.BCrypt;

namespace Repository.Repository
{
    public class UserRepository : RepositoryBase<User>
    {
        private readonly SneakerStoreContext _context;
        private readonly DbSet<User> _dbSet;
        public UserRepository()
        {
            _context = new SneakerStoreContext();
            _dbSet = _context.Set<User>();
        }

        public User Login(string email, string password)
        {
            User user = _dbSet
                .Where(u => u.Email.Equals(email))
                .FirstOrDefault();
            if (user == null || !BC.Verify(password, user.Password))
            {
                return null;
            }
            return user;
        }

        public User GetByEmail(string email)
        {
            User user = _dbSet.Where(u => u.Email.Equals(email)).FirstOrDefault();
            return user;
        }

        public User GetById(long id)
        {
            User user = _dbSet.Where(u => u.Id == id).FirstOrDefault();
            return user;
        }

        public User Register(string email, string phone, string fullName, string password)
        {
            User user = new User() { 
                Email = email,
                Phone = phone,
                Name = fullName,
                Password = BC.HashPassword(password),
                Active = 1,
                Admin = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _dbSet.Add(user);
            _context.SaveChanges();
            return user;
        }


        public User UpdateNameAndPhone(long userId, string phone, string name)
        {
            User user = GetById(userId);
            if (user != null)
            {
                user.Name = name;
                user.Phone = phone;
                user.UpdatedAt = DateTime.Now;
            }
            _dbSet.Update(user);
            _context.SaveChanges();
            return user;
        }

        public User UpdatePassword(long userId, string newPassword)
        {
            User user = GetById(userId);
            if (user != null)
            {
                user.Password = BC.HashPassword(newPassword);
                user.UpdatedAt = DateTime.Now;
            }
            _dbSet.Update(user);
            _context.SaveChanges();
            return user;
        }

        public void ChangeUserStatus(long id)
        {
            User user = GetById(id);
            if (user != null)
            {
                if(user.Active == 0)
                {
                    user.Active = 1;
                } 
                else
                {
                    user.Active = 0;
                }
            }
            _dbSet.Update(user);
            _context.SaveChanges();
        }

        public IQueryable<User> GetAllUserPagination(int page, int size, string search)
        {
            return _dbSet
                .Where(u => (string.IsNullOrEmpty(search) || u.Email.Contains(search)))
                .OrderBy(u => u.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .AsQueryable();
        }

        public int CountAllUserPagination(string search)
        {
            return _dbSet
                .Where(u => (string.IsNullOrEmpty(search) || u.Email.Contains(search)))
                .Count();
        }

        public int CountAllCustomerPagination()
        {
            return _dbSet
                .Where(u => u.Admin == 0)
                .Count();
        }
    }
}
