using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Series.DocumentDB.Infrastructure
{
    public abstract class MongoDbContext : IUnitOfWork
    {
        protected static readonly Dictionary<Type, string> _entityName = new();
        protected readonly IMongoDatabase Database;

        public MongoDbContext(IMongoDatabase database)
        {
            Database = database;
            SetMigrationTypes();
        }

        public virtual void CreateCollection<T>(string name)
        {
            _entityName[typeof(T)] = name;
            if (!Database.ListCollectionNames().ToList().Any(c => c.Equals(name)))
                Database.CreateCollection(name);
        }

        public virtual IMongoCollection<TEntity> GetMongoCollection<TEntity>(string entityName)
        {
            var collectionName = entityName;
            if (!Database.ListCollectionNames().ToList().Any(c => c.Equals(collectionName)))
                CreateCollection<TEntity>(collectionName);

            return Database.GetCollection<TEntity>(collectionName);
        }

        private string GetEntiyName<TEntity>()
        {
            if (!_entityName.ContainsKey(typeof(TEntity)))
                throw new ArgumentNullException(nameof(TEntity));
            var entityName = _entityName[typeof(TEntity)];
            return entityName;
        }

        public virtual IMongoCollection<TEntity> Set<TEntity>()
        {
            var entityName = GetEntiyName<TEntity>();
            return GetMongoCollection<TEntity>(entityName);
        }

        public virtual IMongoCollection<BsonDocument> SetEntityDocument<TEntity>()
        {
            var entityName = GetEntiyName<TEntity>();
            return GetMongoCollection<BsonDocument>(entityName);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected abstract void SetMigrationTypes();
    }
}
