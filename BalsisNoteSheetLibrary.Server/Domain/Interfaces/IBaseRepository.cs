using System.Linq.Expressions;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface IBaseRepository<T> where T : class
{
    public ValueTask<T?> GetByIdAsync(uint id);
    public ValueTask<T?> GetByKeysAsync(params object[] keyValues);
    public Task<List<T>> GetAllAsync();

    public Task<List<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object?>>? orderBy = null,
        bool orderByDescending = false,
        string[]? includeProperties = null,
        bool withTracking = true
        );

    public void Add(T entity);
    public void AddRange(List<T> entities);
    public void Update(T entity);
    public void UpdateRange(List<T> entities);
    public void Remove(T entity);
    public void RemoveRange(List<T> entities);
}