using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public abstract class BaseRepository<T>(AppDbContext dbContext) : IBaseRepository<T> where T : class
{
    public ValueTask<T?> GetByIdAsync(uint id)
    {
        return dbContext.Set<T>().FindAsync(id);
    }
    
    public ValueTask<T?> GetByKeysAsync(params object[] keyValues)
    {
        return dbContext.Set<T>().FindAsync(keyValues);
    }
    
    public Task<List<T>> GetAllAsync()
    {
        return dbContext.Set<T>().ToListAsync();
    }
    
    public void Add(T entity)
    {
        dbContext.Set<T>().Add(entity);
    }
    
    public void AddRange(List<T> entities)
    {
        dbContext.Set<T>().AddRange(entities);
    }
    
    public void Update(T entity)
    {
        dbContext.Set<T>().Update(entity);
    }
    
    public void UpdateRange(List<T> entities)
    {
        dbContext.Set<T>().UpdateRange(entities);
    }
    
    public void Remove(T entity)
    {
        dbContext.Set<T>().Remove(entity);
    }
    
    public void RemoveRange(List<T> entities)
    {
        dbContext.Set<T>().RemoveRange(entities);
    }
}