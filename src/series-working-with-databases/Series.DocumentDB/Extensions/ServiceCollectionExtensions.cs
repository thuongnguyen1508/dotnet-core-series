using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Series.DocumentDB.Infrastructure;
using System;

namespace Series.DocumentDB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static IMongoDatabase CreateMongoDatabase(IServiceProvider serviceProvider, Func<IServiceProvider, object> optionsAction)
        {
            var options = JsonConvert.DeserializeObject<ConnectionConfig>(JsonConvert.SerializeObject(optionsAction.Invoke(serviceProvider)));

            var mongoClient = new MongoClient(options.GetSetting());

            var database = mongoClient.GetDatabase(options.DatabaseName);

            return database;
        }
        public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services, Func<IServiceProvider, object> connectionConfig, ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : MongoDbContext
        {
            services.TryAdd(new ServiceDescriptor(typeof(MongoDatabaseContext<TContext>), p => new MongoDatabaseContext<TContext>(CreateMongoDatabase(p, connectionConfig)), contextLifetime));
            services.AddScoped<TContext>();

            services.AddScoped(sp =>
            {
                var context = sp.GetRequiredService<TContext>();
                return (IUnitOfWork)context;
            });
            return services;
        }
    }
}
