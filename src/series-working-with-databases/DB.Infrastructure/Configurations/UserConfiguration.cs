using DB.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("User");
            builder.HasKey(bill => bill.Id);

            builder.Property(x => x.FullName)
                .HasColumnName("full_name");

            builder.Property(x => x.Birthday)
                 .HasColumnType("timestamp(6) without time zone")
                 .HasColumnName("date_of_birth");
        }
    }
}
