namespace Company.Automation.Contracts.UI;

/// <summary>
/// Manages the Playwright object lifecycle for a single test.
/// IPage and IBrowserContext are intentionally not exposed here to keep Contracts
/// free of the Microsoft.Playwright dependency. Access them via the concrete
/// BrowserSession class in Company.Automation.UI.
/// </summary>
public interface IBrowserSession : IAsyncDisposable
{
    Task InitializeAsync();
}
