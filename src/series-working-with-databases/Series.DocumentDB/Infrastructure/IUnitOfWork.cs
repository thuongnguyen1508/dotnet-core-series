using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Series.DocumentDB.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
