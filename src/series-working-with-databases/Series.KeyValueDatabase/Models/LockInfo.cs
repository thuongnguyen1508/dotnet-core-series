using System;

namespace Series.KeyValueDatabase.Models
{
    public class LockInfo
    {
        public string RequestId { get; set; }

        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime ExpireAt { get; set; }

        public bool IsValid(string requestId)
        {
            return RequestId == requestId;
        }

        public bool IsExpired()
        {
            return ExpireAt < DateTime.Now;
        }
    }
}
