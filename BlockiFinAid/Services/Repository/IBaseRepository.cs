using BlockiFinAid.Data.Models;

namespace BlockiFinAid.Services.Repository;

public interface IBaseRepository<TEntity> where TEntity : IModel
{
    Task<object?> CreateAsync(TEntity model);
    Task<bool> UpdateAsync(TEntity model);
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<object?> StreamInserts();
}