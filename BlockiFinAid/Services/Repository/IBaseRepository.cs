using System.Runtime.CompilerServices;
using BlockiFinAid.Data.Models;

namespace BlockiFinAid.Services.Repository;

public interface IBaseRepository<TEntity> where TEntity : IModel
{
    Task<object?> CreateAsync(TEntity model, string userPerformingAction);
    Task<bool> UpdateAsync(TEntity model, string userPerformingAction);
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> FindByStudentNumberAsync(string studentNumber);

    IAsyncEnumerable<TEntity> StreamInserts(
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
}