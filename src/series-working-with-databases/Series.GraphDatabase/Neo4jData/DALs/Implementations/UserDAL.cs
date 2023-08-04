using AutoMapper;
using Microsoft.Extensions.Logging;
using Series.GraphDatabase.Dtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Series.GraphDatabase.Neo4jData.DALs.Implementations
{
    public class UserDAL : BaseDAL, IUserDAL
    {
        private readonly ILogger<UserDAL> _logger;
        public UserDAL(Neo4jContext context, ILogger<UserDAL> logger, IMapper mapper) : base(context, mapper)
        {
            _logger = logger;
        }

        public Task<bool> SetBasicInforAsync(Guid id, UserDto userDto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
