using Company.Automation.API.Extensions;
using Company.Automation.Configuration.Extensions;
using Company.Automation.Core.Extensions;
using Company.Automation.Core.Logging;
using Company.Automation.Reporting.Extensions;
using Company.Automation.UI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Automation.TestHost;

/// <summary>
/// Builds the DI service provider for test projects.
/// Call <see cref="Create"/> once in [OneTimeSetUp] (or the fixture constructor)
/// and dispose in [OneTimeTearDown].
/// </summary>
public static class TestServiceProvider
{
    /// <summary>
    /// Creates and returns a fully-configured <see cref="IServiceProvider"/>.
    /// appsettings files must be present in the test output directory.
    /// </summary>
    public static IServiceProvider Create(string? basePath = null)
    {
        var configuration = Company.Automation.Configuration.Extensions.ConfigurationExtensions
            .BuildAutomationConfiguration(basePath);

        var services = new ServiceCollection();

        // Make IConfiguration available to anything that needs it
        services.AddSingleton<IConfiguration>(configuration);

        // Framework layers — order matters (each depends on the previous)
        services
            .AddAutomationConfiguration(configuration)
            .AddAutomationCore()
            .AddAutomationReporting()
            .AddAutomationUI()
            .AddAutomationApi();

        return services.BuildServiceProvider();
    }
}
