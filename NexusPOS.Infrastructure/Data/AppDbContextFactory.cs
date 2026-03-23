using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NexusPOS.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "NexusPOS.API");
        var appsettingsPath = Path.Combine(basePath, "appsettings.json");
        
        if (!File.Exists(appsettingsPath))
        {
            throw new FileNotFoundException($"appsettings.json not found at {appsettingsPath}");
        }

        var json = File.ReadAllText(appsettingsPath);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var connectionString = doc.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        return new AppDbContext(optionsBuilder.Options);
    }
}
