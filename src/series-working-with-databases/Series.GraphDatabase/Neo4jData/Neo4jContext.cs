using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;

namespace Series.GraphDatabase.Neo4jData
{
    public class Neo4jContext : IDisposable
    {
        private IAsyncSession _session;
        public string DatabaseName { get; }
        public ICypherFluentQuery Cypher => GraphClient.Cypher.WithDatabase(DatabaseName);
        public IBoltGraphClient GraphClient { get; }
        public IDriver Driver { get; }
        public IAsyncSession AsyncSession => _session;


        public Neo4jContext(IDriver driver, IBoltGraphClient graphClient, string databaseName)
        {
            Driver = driver;
            GraphClient = graphClient;
            DatabaseName = databaseName;

            _session = GetAsyncSession();
        }

        public void Dispose()
        { }

        private IAsyncSession GetAsyncSession()
        {
            return Driver.AsyncSession(sp => sp.WithDatabase(DatabaseName));
        }
    }
}
