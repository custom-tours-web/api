using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace it;

/// <summary>
/// Custom WebApplicationFactory for integration testing the API.
/// Overrides application configuration, database contexts, background services,
/// and logging to ensure isolated, repeatable test execution.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    #region Properties

    /// <summary>
    /// Unique database instance identifier generated per factory instance to prevent cross-test data pollution.
    /// </summary>
    public string DatabaseName { get; } = $"TestDb_{Guid.NewGuid()}";

    #endregion

    #region Host Configuration

    /// <summary>
    /// Configures the test web host with testing configuration, isolated database providers, and test doubles.
    /// </summary>
    /// <param name="builder">The web host builder to configure for testing.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Force the execution environment to "Testing"
        builder.UseEnvironment("Testing");

        // 2. Override settings using an in-memory configuration collection
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Information",
                ["ConnectionStrings:DefaultConnection"] = $"Host=localhost;Database={DatabaseName};",
                ["Jwt:Secret"] = "SUPER_SECRET_INTEGRATION_TEST_KEY_THAT_IS_AT_LEAST_256_BITS_LONG",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            });
        });

        // 3. Restrict and format logging output during test execution
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        // 4. Swap production infrastructure for test doubles using TestHost
        builder.ConfigureTestServices(services =>
        {
            // Remove production DbContextOptions registration if present
            RemoveServiceDescriptor<DbContextOptions>(services);

            // Remove background workers to prevent async tasks from running during integration tests
            RemoveHostedServices(services);

            // Register mock/fake services or custom authentication handlers here
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Utility method to remove an existing service descriptor from the service collection.
    /// </summary>
    private static void RemoveServiceDescriptor<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }

    /// <summary>
    /// Utility method to remove all IHostedService background workers.
    /// </summary>
    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
        foreach (var service in hostedServices)
        {
            services.Remove(service);
        }
    }

    #endregion
}
