using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Series.GraphDatabase.Neo4jData.DALs.Implementations
{
    public abstract class BaseDAL : IBaseDAL
    {
        protected readonly Neo4jContext _context;
        protected readonly IMapper _mapper;

        public BaseDAL(Neo4jContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<T[]> GetNodesAsync<T>(string label, KeyValuePair<string, object> uniquePropertyVale, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _context.Cypher.Read.OptionalMatch($"(n:{label} {{ {uniquePropertyVale.Key}: $value }})")
                .WithParam("value", uniquePropertyVale.Value)
                .Return<string>("n").ResultsAsync;

            //var cypher = _context.Cypher.Read.OptionalMatch($"(n:{label} {{ {uniquePropertyVale.Key}: $value }})")
            //    .WithParam("value", uniquePropertyVale.Value)
            //    .Return<T>("n");

            //var result = await cypher.ResultsAsync;

            if (result == null)
                return System.Array.Empty<T>();

            return result.Select(c =>
            {
                var data = JsonConvert.DeserializeObject<JObject>(c);

                var payload = data["data"] as JObject;

                return payload.ToObject<T>();

            }).ToArray();
        }

        public async Task<bool> IsNodeExistedAsync(string label, KeyValuePair<string, object> uniquePropertyVale, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cypher = _context.Cypher.Read.OptionalMatch($"(n:{label} {{ {uniquePropertyVale.Key}: $value }})")
                .WithParam("value", uniquePropertyVale.Value)
                .Where("n.createdAt is not null")
                .With("n IS NOT NULL AS result")
                .Return<bool>("result");

            var result = await cypher.ResultsAsync;

            if (result == null)
                return false;

            return result.FirstOrDefault();
        }

        public Task RemoveNodeAsync(string label, KeyValuePair<string, object> uniquePropertyVale, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cypher = _context.Cypher.Write
                .Match($"(a:{label} {{ {uniquePropertyVale.Key}: $value }})")
                .WithParam("value", uniquePropertyVale.Value)
                .DetachDelete("a");
            return cypher.ExecuteWithoutResultsAsync();
        }
    }
}
