namespace Series.DocumentDB.Infrastructure
{
    public interface IEntity
    {
        object[] GetKeys();

        bool IsTransient();
    }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; }
    }
}
