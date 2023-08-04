using Series.GraphDatabase.Dtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Series.GraphDatabase.Neo4jData.DALs
{
    public interface IUserDAL
    {
        Task<bool> SetBasicInforAsync(Guid id, UserDto userDto, CancellationToken cancellationToken = default);
    }
}
