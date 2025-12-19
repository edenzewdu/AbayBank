using AbayBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbayBank.Persistence.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .HasColumnType("char(36)");

            builder.Property(a => a.AccountNumber)
                   .HasColumnType("varchar(20)")
                   .IsRequired();

            builder.Property(a => a.Balance)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(a => a.Status)
                   .HasConversion<int>() // Enum as int
                   .IsRequired();
        }
    }
}
