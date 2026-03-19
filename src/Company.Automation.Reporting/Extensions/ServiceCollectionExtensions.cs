using Microsoft.Extensions.DependencyInjection;

namespace Company.Automation.Reporting.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers reporting services. Currently a no-op placeholder since AllureHelper is static,
    /// but provides the extension point if a scoped reporting service is added in the future.
    /// </summary>
    public static IServiceCollection AddAutomationReporting(this IServiceCollection services)
    {
        return services;
    }
}
