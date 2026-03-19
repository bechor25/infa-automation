using Company.Automation.UI.Driver;
using Company.Automation.UI.Pages;

namespace Company.Automation.SampleTests.UI.Pages;

/// <summary>
/// Page object for the secure dashboard page shown after successful login.
/// Demonstrates reading text, checking visibility, and navigation.
/// </summary>
public sealed class DashboardPage : BasePage
{
    private const string LogoutLink    = "a[href='/logout']";
    private const string FlashMessage  = "#flash";
    private const string SecureContent = "h2";

    protected override string GetRelativePath() => "/secure";

    public DashboardPage(PlaywrightDriver driver, string baseUrl)
        : base(driver, baseUrl) { }

    public async Task<bool> IsLoggedInAsync() =>
        await Driver.IsVisibleAsync(LogoutLink);

    public async Task<string> GetWelcomeHeadingAsync() =>
        await Driver.GetTextAsync(SecureContent);

    public async Task LogoutAsync() =>
        await Driver.ClickAsync(LogoutLink);

    public override async Task WaitForPageReadyAsync() =>
        await Driver.WaitForVisibleAsync(SecureContent);
}
