using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll.BoDi;

namespace IT;

/// <summary>
/// Provides shared HTTP client and WebApplicationFactory setup for API integration tests.
/// Manages lifecycle and dependency injection of test infrastructure across Reqnroll scenarios.
/// </summary>
[Binding]
public class TestHooks
{
    private static CustomWebApplicationFactory? _factory;

    /// <summary>
    /// Thread-safe accessor for the global web application factory instance.
    /// </summary>
    public static CustomWebApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("Test infrastructure factory has not been initialized by [BeforeTestRun].");

    /// <summary>
    /// Bootstraps application-wide test infrastructure once before any test scenario runs.
    /// </summary>
    [BeforeTestRun(Order = 0)]
    public static async Task GlobalSetupAsync()
    {
        _factory = new CustomWebApplicationFactory();

        // Performs initial async initialization (e.g., seeding data or applying database migrations)
        await Task.CompletedTask;
    }

    /// <summary>
    /// Registers scenario-scoped HTTP clients and options into the Reqnroll IoC container.
    /// </summary>
    /// <param name="objectContainer">The scenario-level BoDi dependency injection container.</param>
    [BeforeScenario(Order = 0)]
    public static void SetupScenarioServices(IObjectContainer objectContainer)
    {
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        };

        var client = Factory.CreateClient(clientOptions);

        objectContainer.RegisterInstanceAs(client);
        objectContainer.RegisterInstanceAs(Factory);
    }

    /// <summary>
    /// Tag-specific hook executed before scenarios marked with the '@database' tag.
    /// </summary>
    [BeforeScenario("database", Order = 1)]
    public static async Task ResetDatabaseStateAsync()
    {
        // Executes asynchronous state reset (e.g., Respawn or EF Core database checkpointing)
        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes scenario-level resources immediately after scenario completion.
    /// </summary>
    /// <param name="objectContainer">The scenario-level BoDi container.</param>
    [AfterScenario]
    public static void TeardownScenario(IObjectContainer objectContainer)
    {
        if (objectContainer.IsRegistered<HttpClient>())
        {
            var client = objectContainer.Resolve<HttpClient>();
            client.Dispose();
        }
    }

    /// <summary>
    /// Cleans up global test infrastructure after all test scenarios have executed.
    /// </summary>
    [AfterTestRun]
    public static async Task GlobalTeardownAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }
    }
}
