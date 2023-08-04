using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Series.DocumentDB.Infrastructure
{
    public abstract class MongoDbContext : IUnitOfWork
    {
        #region Db collection tracker
        public enum State
        {
            Detached = 0,
            Unchanged = 1,
            Deleted = 2,
            Modified = 3,
            Added = 4
        }

        protected class ChangedEntity
        {
            public IEntity Entity { get; set; }
            public State State { get; set; }
            public virtual Type EntityType => Entity?.GetType();

            public virtual Func<CancellationToken, Task<bool>> Execute { get; set; }
        }
        #endregion

        protected static readonly Dictionary<Type, string> _entityName = new();
        protected readonly List<ChangedEntity> _changedEntities = new();
        protected readonly IMongoDatabase Database;

        public MongoDbContext(IMongoDatabase database)
        {
            Database = database;
            SetMigrationTypes();
        }

        #region Db manage collection
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

        public virtual void Dispose() { }
        protected abstract void SetMigrationTypes();
        #endregion

        #region IUnitOfWork
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<(int index, bool isSucess)>();
            foreach (var item in _changedEntities)
            {
                result.Add((_changedEntities.IndexOf(item), await item.Execute(cancellationToken)));
            }
            var removeIndex = result.Where(r => r.isSucess).Select(r => r.index);
            foreach (var index in removeIndex)
            {
                _changedEntities.RemoveAt(index);
            }

            return result.Count(r => r.isSucess);
        }
        #endregion

        #region Db manipulation operation
        public virtual TEntity Add<TEntity>(TEntity entity) where TEntity : IEntity
        {
            if (!entity.IsTransient())
                _changedEntities.Add(new ChangedEntity
                {
                    Entity = entity,
                    State = State.Added,
                    Execute = async (token) =>
                    {
                        var entityCollection = Set<TEntity>();
                        await entityCollection.InsertOneAsync(entity, cancellationToken: token);
                        return true;
                    }
                });

            return entity;
        }

        public virtual IEnumerable<TEntity> AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : IEntity
        {
            foreach (var entity in entities)
            {
                if (entity.IsTransient())
                    continue;
                _changedEntities.Add(new ChangedEntity
                {
                    Entity = entity,
                    State = State.Added,
                    Execute = async (token) =>
                    {
                        var entityCollection = Set<TEntity>();
                        await entityCollection.InsertOneAsync(entity, cancellationToken: token);
                        return true;
                    }
                });
                yield return entity;
            }
        }

        public virtual void Update<TEntity>(FilterDefinition<TEntity> filterDefinition, TEntity entity) where TEntity : IEntity
        {
            _changedEntities.Add(new ChangedEntity
            {
                Entity = entity,
                State = State.Modified,
                Execute = async (token) =>
                {
                    var entityCollection = Set<TEntity>();
                    await entityCollection.ReplaceOneAsync(filterDefinition, entity, new ReplaceOptions { IsUpsert = true }, cancellationToken: token);
                    return true;
                }
            });
        }

        public virtual void Update<TEntity>(Expression<Func<TEntity, bool>> filterExpression, TEntity entity) where TEntity : IEntity
        {
            var filter = Builders<TEntity>.Filter.Where(filterExpression);
            _changedEntities.Add(new ChangedEntity
            {
                Entity = entity,
                State = State.Modified,
                Execute = async (token) =>
                {
                    var entityCollection = Set<TEntity>();
                    await entityCollection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true }, cancellationToken: token);
                    return true;
                }
            });
        }

        public virtual void Update<TEntity>(Expression<Func<TEntity, bool>> filterExpression, UpdateDefinition<TEntity> updateDefinition) where TEntity : IEntity
        {
            _changedEntities.Add(new ChangedEntity
            {
                State = State.Modified,
                Execute = async (token) =>
                {
                    var entityCollection = Set<TEntity>();
                    await entityCollection.FindOneAndUpdateAsync(filterExpression, updateDefinition, cancellationToken: token);
                    return true;
                }
            });
        }

        public virtual void Delete<TEntity>(Expression<Func<TEntity, bool>> filterExpression) where TEntity : IEntity
        {
            var filter = Builders<TEntity>.Filter.Where(filterExpression);
            _changedEntities.Add(new ChangedEntity
            {
                State = State.Deleted,
                Execute = async (token) =>
                {
                    var entityCollection = Set<TEntity>();
                    await entityCollection.DeleteOneAsync(filter, token);
                    return true;
                }
            });
        }
        #endregion
    }
}
