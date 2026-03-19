using Company.Automation.Configuration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Automation.Configuration.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Builds an <see cref="IConfiguration"/> from appsettings files and environment variables.
    /// Environment is resolved from the ASPNETCORE_ENVIRONMENT or AUTOMATION_ENVIRONMENT variable.
    /// </summary>
    public static IConfiguration BuildAutomationConfiguration(string? basePath = null)
    {
        basePath ??= AppContext.BaseDirectory;

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("AUTOMATION_ENVIRONMENT")
                          ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("AUTOMATION_")
            .Build();
    }

    /// <summary>
    /// Registers <see cref="AutomationSettings"/> and its sub-sections with the DI container.
    /// </summary>
    public static IServiceCollection AddAutomationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AutomationSettings>(configuration.GetSection(AutomationSettings.SectionName));
        services.Configure<BrowserSettings>(
            configuration.GetSection($"{AutomationSettings.SectionName}:{nameof(AutomationSettings.Browser)}"));
        services.Configure<ApiSettings>(
            configuration.GetSection($"{AutomationSettings.SectionName}:{nameof(AutomationSettings.Api)}"));
        services.Configure<ReportSettings>(
            configuration.GetSection($"{AutomationSettings.SectionName}:{nameof(AutomationSettings.Report)}"));

        return services;
    }
}
