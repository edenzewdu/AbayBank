using AbayBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbayBank.Persistence.Context;

public class AbayBankDbContext : DbContext
{
    public AbayBankDbContext(DbContextOptions<AbayBankDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Account>(entity =>
    {
        entity.Property(e => e.Id)
            .HasColumnType("char(36)") // MySQL stores Guid as char(36)
            .IsRequired();

        entity.Property(e => e.AccountNumber)
            .HasColumnType("varchar(20)")
            .IsRequired();

        entity.Property(e => e.Balance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.Property(e => e.Status)
            .HasColumnType("int")
            .IsRequired();
    });

    modelBuilder.Entity<Transaction>(entity =>
    {
        entity.Property(e => e.Id)
            .HasColumnType("char(36)")
            .IsRequired();

        entity.Property(e => e.AccountId)
            .HasColumnType("char(36)")
            .IsRequired();

        entity.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.Property(e => e.Type)
            .HasColumnType("int")
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();
    });

    base.OnModelCreating(modelBuilder);
}

}
