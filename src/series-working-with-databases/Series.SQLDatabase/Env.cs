using System;

namespace Series.SQLDatabase
{
    public static class Env
    {
        //public readonly static string DB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        public readonly static string DB_CONNECTION_STRING = "Host=host.docker.internal;Port=5432;Database=chatdb;Username=postgres;Password=t0ps3cr3t";
    }
}
