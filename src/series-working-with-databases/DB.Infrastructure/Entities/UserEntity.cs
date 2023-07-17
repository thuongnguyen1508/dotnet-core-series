using System;

namespace DB.Infrastructure.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime Birthday { get; set; }
    }
}
