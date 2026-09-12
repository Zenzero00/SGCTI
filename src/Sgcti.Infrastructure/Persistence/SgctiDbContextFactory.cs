using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sgcti.Infrastructure.Persistence;

public class SgctiDbContextFactory : IDesignTimeDbContextFactory<SgctiDbContext>
{
    public SgctiDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "Sgcti.Api"));

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SgctiDbContext>();
        var connectionString = configuration.GetConnectionString("SgctiDB");

        optionsBuilder.UseNpgsql(connectionString ?? "Host=localhost;Database=sgcti;Username=postgres;Password=postgres;");

        return new SgctiDbContext(optionsBuilder.Options);
    }
}