using System.Linq.Expressions;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public abstract class BaseRepository<T>(AppDbContext context) : IBaseRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public ValueTask<T?> GetByIdAsync(uint id)
    {
        return DbSet.FindAsync(id);
    }
    
    public ValueTask<T?> GetByKeysAsync(params object[] keyValues)
    {
        return DbSet.FindAsync(keyValues);
    }

    public Task<List<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object?>>? orderBy = null,
        bool orderByDescending = false,
        string[]? includeProperties = null,
        bool withTracking = true)
    {
        IQueryable<T> query = DbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
        }

        if (orderBy != null)
        {
            query = orderByDescending 
                ? query.OrderByDescending(orderBy) 
                : query.OrderBy(orderBy);
        }

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query.ToListAsync();
    }

    
    public Task<List<T>> GetAllAsync()
    {
        return DbSet.ToListAsync();
    }
    
    public void Add(T entity)
    {
        DbSet.Add(entity);
    }
    
    public void AddRange(List<T> entities)
    {
        DbSet.AddRange(entities);
    }
    
    public void Update(T entity)
    {
        DbSet.Update(entity);
    }
    
    public void UpdateRange(List<T> entities)
    {
        DbSet.UpdateRange(entities);
    }
    
    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
    
    public void RemoveRange(List<T> entities)
    {
        DbSet.RemoveRange(entities);
    }
}