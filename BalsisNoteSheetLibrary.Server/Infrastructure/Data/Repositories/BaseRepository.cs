using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public abstract class BaseRepository<T>(AppDbContext context) : IBaseRepository<T>
    where T : class
{
    protected readonly AppDbContext DbContext = context;

    public ValueTask<T?> GetByIdAsync(uint id)
    {
        return DbContext.Set<T>().FindAsync(id);
    }
    
    public ValueTask<T?> GetByKeysAsync(params object[] keyValues)
    {
        return DbContext.Set<T>().FindAsync(keyValues);
    }
    
    public Task<List<T>> GetAllAsync()
    {
        return DbContext.Set<T>().ToListAsync();
    }
    
    public void Add(T entity)
    {
        DbContext.Set<T>().Add(entity);
    }
    
    public void AddRange(List<T> entities)
    {
        DbContext.Set<T>().AddRange(entities);
    }
    
    public void Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
    }
    
    public void UpdateRange(List<T> entities)
    {
        DbContext.Set<T>().UpdateRange(entities);
    }
    
    public void Remove(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }
    
    public void RemoveRange(List<T> entities)
    {
        DbContext.Set<T>().RemoveRange(entities);
    }
}