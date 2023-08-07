using System;

namespace Series.KeyValueDatabase
{
    public static class Env
    {
        public readonly static bool REDIS_SSL = bool.TryParse(Environment.GetEnvironmentVariable("REDIS_SSL"), out bool isSsl) && isSsl;
        public readonly static string REDIS_HOST = Environment.GetEnvironmentVariable("REDIS_HOST");
        public readonly static string REDIS_PASSWORD = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        public readonly static int REDIS_PORT = int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out int redisPort) ? redisPort : 6379;
    }
}
