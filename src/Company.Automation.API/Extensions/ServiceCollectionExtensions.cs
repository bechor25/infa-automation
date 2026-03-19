using Company.Automation.API.Client;
using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Company.Automation.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the API client and its HttpClient via IHttpClientFactory.
    /// The named client "AutomationApi" is used internally.
    /// </summary>
    public static IServiceCollection AddAutomationApi(this IServiceCollection services)
    {
        services.AddHttpClient<ApiClient>("AutomationApi", (provider, client) =>
        {
            var settings = provider.GetRequiredService<IOptions<ApiSettings>>().Value;
            client.BaseAddress = string.IsNullOrEmpty(settings.BaseUrl)
                ? null
                : new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // Expose the typed client as IApiClient for DI consumers
        services.AddTransient<IApiClient, ApiClient>();

        return services;
    }
}
