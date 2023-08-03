using MongoDB.Driver;

namespace Series.DocumentDB.Infrastructure
{
    public class MongoDatabaseContext<TContext> where TContext : MongoDbContext
    {
        public MongoDatabaseContext(IMongoDatabase database)
        {
            Database = database;
        }
        public IMongoDatabase Database { get; }
    }
}
