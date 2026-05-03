using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrivateECommerce.API.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration["DATABASE_URL"]
                ?? Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string not found. Set ConnectionStrings:DefaultConnection or DATABASE_URL.");
            }

            if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':', 2);

                connectionString =
                    $"Host={uri.Host};" +
                    $"Port={uri.Port};" +
                    $"Database={uri.AbsolutePath.TrimStart('/')};" +
                    $"Username={Uri.UnescapeDataString(userInfo[0])};" +
                    $"Password={Uri.UnescapeDataString(userInfo[1])};" +
                    "SSL Mode=Require;" +
                    "Trust Server Certificate=true";
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
