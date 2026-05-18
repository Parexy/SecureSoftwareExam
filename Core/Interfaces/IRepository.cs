namespace PatientJournal.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();

    Task<T?> GetAsync(int id);

    Task AddAsync(T entity);

    Task EditAsync(T entity);

    Task DeleteAsync(int id);
}