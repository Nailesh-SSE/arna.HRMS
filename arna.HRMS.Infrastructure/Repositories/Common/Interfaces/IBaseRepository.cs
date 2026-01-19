using arna.HRMS.Core.Common;

namespace arna.HRMS.Infrastructure.Repositories.Common.Interfaces;

public interface IBaseRepository<T> where T : class, IBaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    IQueryable<T> Query();
}
