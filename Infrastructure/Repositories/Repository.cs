using Microsoft.EntityFrameworkCore;
using PatientJournal.Core.Interfaces;

namespace PatientJournal.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly PatientJournalContext context;
    private readonly DbSet<T> dbSet;

    public Repository(PatientJournalContext context)
    {
        this.context = context;
        dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    public async Task<T?> GetAsync(int id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task EditAsync(T entity)
    {
        dbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetAsync(id);

        if (entity == null)
        {
            return;
        }

        dbSet.Remove(entity);
        await context.SaveChangesAsync();
    }
}