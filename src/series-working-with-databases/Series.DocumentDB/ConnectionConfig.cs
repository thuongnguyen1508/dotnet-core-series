using MongoDB.Driver;

namespace Series.DocumentDB
{
    public class ConnectionConfig
    {
        public string ConnectionString { get; set; }
        public string Version { get; set; }
        public string DatabaseName { get; set; }

        public MongoClientSettings GetSetting()
        {
            var setting = MongoClientSettings.FromConnectionString(ConnectionString);
            return setting;

        }
    }
}
