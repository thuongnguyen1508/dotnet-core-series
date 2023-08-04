using Series.DocumentDB.Infrastructure;

namespace Series.DocumentDB.Data.Entities
{
    public class UserEntity : IEntity<string>
    {
        public const string SCHEMA_NAME = "Users";

        public string Id { get; set; }

        public object[] GetKeys() => new object[] { Id };

        public bool IsTransient() => string.IsNullOrWhiteSpace(Id);
    }
}
