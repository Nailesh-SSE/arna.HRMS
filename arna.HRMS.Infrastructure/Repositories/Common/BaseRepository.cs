using arna.HRMS.Core.Common;
using arna.HRMS.Infrastructure.Data;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories.Common;

public class BaseRepository<T> : IBaseRepository<T> where T : class, IBaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var trackedEntity = _context.ChangeTracker
            .Entries<T>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntity != null)
        {
            trackedEntity.State = EntityState.Detached;
        }

        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;

        await _context.SaveChangesAsync();
        return entity;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

}
