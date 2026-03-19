using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.UI;
using Company.Automation.UI.Browser;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Automation.UI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all UI automation services.
    /// BrowserSession is Transient so each resolution creates a new independent session,
    /// allowing parallel tests to have separate browser contexts.
    /// </summary>
    public static IServiceCollection AddAutomationUI(this IServiceCollection services)
    {
        // Transient: each test resolves its own BrowserSession
        services.AddTransient<BrowserSession>();

        return services;
    }
}
