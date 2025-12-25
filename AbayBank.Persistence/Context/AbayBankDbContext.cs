using Microsoft.EntityFrameworkCore;
using AbayBank.Domain.Entities;

namespace AbayBank.Persistence.Context
{
    public class AbayBankDbContext : DbContext
    {
        public AbayBankDbContext(DbContextOptions<AbayBankDbContext> options)
            : base(options)
        {
        }

        // Add parameterless constructor for design-time
        protected AbayBankDbContext()
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity mappings here
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AbayBankDbContext).Assembly);
        }
    }
}