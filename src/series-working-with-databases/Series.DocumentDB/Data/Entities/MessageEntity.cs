using Series.DocumentDB.Infrastructure;

namespace Series.DocumentDB.Data.Entities
{
    public class MessageEntity : IEntity<string>
    {
        public const string SCHEMA_NAME = "Messages";

        public string Id { get; set; }

        public object[] GetKeys() => new object[] { Id };

        public bool IsTransient() => !string.IsNullOrWhiteSpace(Id);
    }
}
