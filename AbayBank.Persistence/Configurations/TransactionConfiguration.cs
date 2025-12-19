using AbayBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbayBank.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnType("char(36)");

            builder.Property(t => t.AccountId)
                   .HasColumnType("char(36)")
                   .IsRequired();

            builder.Property(t => t.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(t => t.Type)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(t => t.CreatedAt)
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasOne<Account>()
                   .WithMany()
                   .HasForeignKey(t => t.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
