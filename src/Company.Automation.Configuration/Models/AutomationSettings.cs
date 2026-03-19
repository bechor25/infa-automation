namespace Company.Automation.Configuration.Models;

/// <summary>
/// Root configuration object. Bound from the "Automation" section in appsettings.json.
/// Register with DI using <see cref="Extensions.ConfigurationExtensions.AddAutomationConfiguration"/>.
/// </summary>
public sealed class AutomationSettings
{
    public const string SectionName = "Automation";

    /// <summary>Name of the environment being tested (e.g. Development, QA, Production).</summary>
    public string Environment { get; init; } = "Development";

    /// <summary>Base URL for UI tests.</summary>
    public string UiBaseUrl { get; init; } = string.Empty;

    public BrowserSettings Browser { get; init; } = new();
    public ApiSettings Api { get; init; } = new();
    public ReportSettings Report { get; init; } = new();
}
