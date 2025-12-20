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
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AccountNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.AccountNumber).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ReferenceNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}