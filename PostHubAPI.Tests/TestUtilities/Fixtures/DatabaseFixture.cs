using Microsoft.EntityFrameworkCore;
using PostHubAPI.Data;

namespace PostHubAPI.Tests.TestUtilities.Fixtures;

/// <summary>
/// Provides a fresh in-memory database for each test
/// </summary>
public class DatabaseFixture : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    /// <summary>
    /// Creates a new ApplicationDbContext with a unique in-memory database
    /// </summary>
    public ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(_options);
    }

    /// <summary>
    /// Creates a fresh in-memory database for isolated testing
    /// </summary>
    public static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }
}
