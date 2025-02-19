using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly WorksmartDBContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(WorksmartDBContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
