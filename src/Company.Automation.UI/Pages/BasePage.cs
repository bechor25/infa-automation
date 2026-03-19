using Company.Automation.UI.Driver;

namespace Company.Automation.UI.Pages;

/// <summary>
/// Base class for all Page Objects.
///
/// Naming convention for locator constants (define in each page class):
///   private const string LoginButton = "#login-btn";
///   private const string UsernameInput = "[data-testid='username']";
///
/// Each page object gets a Driver and the UI base URL from the framework.
/// Methods should describe business actions, not technical steps:
///   public Task LoginAsync(string user, string pass)  ✓
///   public Task ClickLoginButtonAsync()               ✗ (too granular for page API)
/// </summary>
public abstract class BasePage
{
    protected PlaywrightDriver Driver { get; }
    protected string BaseUrl { get; }

    protected BasePage(PlaywrightDriver driver, string baseUrl)
    {
        Driver = driver;
        BaseUrl = baseUrl;
    }

    /// <summary>
    /// Navigate to this page's path. Override to set the relative path.
    /// </summary>
    public virtual async Task OpenAsync()
    {
        var path = GetRelativePath();
        var url = string.IsNullOrEmpty(path)
            ? BaseUrl
            : $"{BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        await Driver.NavigateAsync(url);
    }

    /// <summary>Returns the relative path for this page (e.g. "/login"). Override as needed.</summary>
    protected virtual string GetRelativePath() => string.Empty;

    /// <summary>
    /// Waits until the page is in a stable, recognisable state.
    /// Override to wait for a specific landmark element that confirms the page loaded.
    /// </summary>
    public virtual Task WaitForPageReadyAsync() =>
        Driver.WaitForPageLoadAsync("networkidle");

    /// <summary>Takes and attaches a named screenshot to the Allure report.</summary>
    protected Task ScreenshotAsync(string name) =>
        Driver.AttachScreenshotToReportAsync(name);
}
