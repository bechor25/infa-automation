namespace Company.Automation.Configuration.Models;

public sealed class ApiSettings
{
    /// <summary>Default base URL applied to all relative request paths.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>Number of retry attempts for transient HTTP failures (5xx, timeout).</summary>
    public int RetryCount { get; init; } = 2;

    /// <summary>Delay between retries in milliseconds.</summary>
    public int RetryDelayMs { get; init; } = 300;

    /// <summary>Attach full request body to Allure report.</summary>
    public bool AttachRequestToReport { get; init; } = true;

    /// <summary>Attach full response body to Allure report.</summary>
    public bool AttachResponseToReport { get; init; } = true;

    /// <summary>Named additional base URLs for multi-service test suites.</summary>
    public IDictionary<string, string> AdditionalBaseUrls { get; init; } = new Dictionary<string, string>();
}
