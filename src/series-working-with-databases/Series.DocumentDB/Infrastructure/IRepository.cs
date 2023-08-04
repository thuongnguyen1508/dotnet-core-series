using System.Threading.Tasks;

namespace Series.DocumentDB.Infrastructure
{
    public interface IRepository<T> where T : IEntity
    {

        Task<T> GetByIdAsync(string id);

        Task<T> AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);
    }
}
