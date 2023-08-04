using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Series.DocumentDB.Data.Entities;

namespace Series.DocumentDB.Data
{
    public static class MongoConfigurator
    {
        public static void Configure()
        {
            // Register common conventions for all collections
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };

            ConventionRegistry.Register("Camel case convention", pack, t => true);

            BsonClassMap.RegisterClassMap<MessageEntity>(classMap =>
            {
                classMap.AutoMap();
                classMap.SetIgnoreExtraElements(true);
                classMap.MapIdProperty(c => c.Id);
            });

            BsonClassMap.RegisterClassMap<UserEntity>(classMap =>
            {
                classMap.AutoMap();
                classMap.SetIgnoreExtraElements(true);
                classMap.MapIdProperty(c => c.Id);
            });
        }
    }
}
