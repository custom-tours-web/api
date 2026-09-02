using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll.BoDi;

namespace it;

/// <summary>
/// Provides shared HTTP client and WebApplicationFactory setup for API integration tests.
/// </summary>
[Binding]
public class TestHooks
{
    #region Fields & Properties

    private static CustomWebApplicationFactory? _factory;

    /// <summary>
    /// Thread-safe accessor for the global web application factory instance.
    /// </summary>
    public static CustomWebApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("Test infrastructure factory has not been initialized by [BeforeTestRun].");

    #endregion

    #region Global Lifecycle (Test Run)

    /// <summary>
    /// Bootstraps application-wide test infrastructure once before any test scenario runs.
    /// </summary>
    [BeforeTestRun(Order = 0)]
    public static async Task GlobalSetupAsync()
    {
        _factory = new CustomWebApplicationFactory();

        await Task.CompletedTask;
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

    #endregion

    #region Scenario Lifecycle (Per Test)

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

    #endregion
}
