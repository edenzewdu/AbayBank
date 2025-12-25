using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;  // Add this using
using System.IO;

namespace AbayBank.Persistence.Context
{
    public class AbayBankDbContextFactory : IDesignTimeDbContextFactory<AbayBankDbContext>
    {
        public AbayBankDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure DbContext
            var optionsBuilder = new DbContextOptionsBuilder<AbayBankDbContext>();
            optionsBuilder.UseMySQL(connectionString);

            return new AbayBankDbContext(optionsBuilder.Options);
        }
    }
}