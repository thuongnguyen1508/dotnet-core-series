using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Series.KeyValueDatabase.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IRedisDatabase _cache;
    }
}
