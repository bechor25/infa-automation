using Company.Automation.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Company.Automation.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core framework services: logging factory and ILogger<T> support.
    /// </summary>
    public static IServiceCollection AddAutomationCore(this IServiceCollection services)
    {
        var loggerFactory = AutomationLoggerFactory.Create();

        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }
}
