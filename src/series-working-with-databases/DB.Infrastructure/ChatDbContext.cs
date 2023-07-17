using DB.Infrastructure.Configurations;
using DB.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DB.Infrastructure
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
        public DbSet<UserEntity> Users { get; set; }
    }
}
