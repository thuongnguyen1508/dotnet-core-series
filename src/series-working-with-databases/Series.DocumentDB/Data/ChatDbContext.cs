using MongoDB.Driver;
using Series.DocumentDB.Data.Entities;
using Series.DocumentDB.Infrastructure;
using System;

namespace Series.DocumentDB.Data
{
    public class ChatDbContext : MongoDbContext
    {
        public ChatDbContext(MongoDatabaseContext<ChatDbContext> mongoDatabase) : base(mongoDatabase.Database)
        {

        }

        public IMongoCollection<UserEntity> Users => Set<UserEntity>();
        public IMongoCollection<MessageEntity> Messages => Set<MessageEntity>();

        protected override void SetMigrationTypes()
        {
            _entityName[typeof(UserEntity)] = UserEntity.SCHEMA_NAME;
            _entityName[typeof(MessageEntity)] = UserEntity.SCHEMA_NAME;
        }
    }
}
