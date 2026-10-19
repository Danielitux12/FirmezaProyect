using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Interfaces;

/// <summary>
/// Generic base repository interface defining standard CRUD operations.
/// </summary>
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
}
