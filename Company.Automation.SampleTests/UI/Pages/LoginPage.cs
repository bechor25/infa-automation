using Company.Automation.UI.Driver;
using Company.Automation.UI.Pages;

namespace Company.Automation.SampleTests.UI.Pages;

/// <summary>
/// Page object for the login page.
///
/// Locators are defined as private constants.
/// Public methods describe business actions, not individual UI interactions.
/// The framework handles waits, retries, logging, and Allure steps internally.
/// </summary>
public sealed class LoginPage : BasePage
{
    // ── Locators ─────────────────────────────────────────────────────────────
    private const string UsernameInput = "#username";
    private const string PasswordInput = "#password";
    private const string LoginButton   = "button[type='submit']";
    private const string ErrorMessage  = "#flash.error";
    private const string SuccessMessage = "#flash.success";

    protected override string GetRelativePath() => "/login";

    public LoginPage(PlaywrightDriver driver, string baseUrl)
        : base(driver, baseUrl) { }

    // ── Business actions ─────────────────────────────────────────────────────

    public async Task LoginAsync(string username, string password)
    {
        await Driver.FillAsync(UsernameInput, username);
        await Driver.FillAsync(PasswordInput, password);
        await Driver.ClickAsync(LoginButton);
    }

    public async Task<bool> IsErrorDisplayedAsync() =>
        await Driver.IsVisibleAsync(ErrorMessage);

    public async Task<string> GetErrorMessageAsync() =>
        await Driver.GetTextAsync(ErrorMessage);

    public async Task<string> GetSuccessMessageAsync() =>
        await Driver.GetTextAsync(SuccessMessage);

    public override async Task WaitForPageReadyAsync() =>
        await Driver.WaitForVisibleAsync(UsernameInput);
}
