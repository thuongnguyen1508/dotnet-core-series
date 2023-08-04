using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Series.GraphDatabase.Neo4jData.DALs
{
    public interface IBaseDAL
    {
        Task<T[]> GetNodesAsync<T>(string label, KeyValuePair<string, object> uniquePropertyVale, CancellationToken cancellationToken = default);
        Task<bool> IsNodeExistedAsync(string label, KeyValuePair<string, object> uniquePropertyVale, CancellationToken cancellationToken = default);
        Task RemoveNodeAsync(string label, KeyValuePair<string, object> uniquePropertyVale, CancellationToken cancellationToken = default);
    }
}
