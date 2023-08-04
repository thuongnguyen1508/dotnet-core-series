using Series.DocumentDB.Data;
using Series.DocumentDB.Data.Entities;
using System;
using System.Threading.Tasks;

namespace Series.DocumentDB.Repository.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatDbContext _context;
        public UserRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity> AddAsync(UserEntity entity)
        {
            _context.Add<UserEntity>(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public Task DeleteAsync(UserEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserEntity> GetByIdAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateAsync(UserEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
