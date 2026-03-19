using Allure.NUnit;
using Allure.NUnit.Attributes;
using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.API;
using Company.Automation.Reporting;
using Company.Automation.UI.Browser;
using Company.Automation.UI.Driver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Company.Automation.TestHost;

/// <summary>
/// Base class for UI-only test fixtures.
///
/// Browser lifecycle:
///   [OneTimeSetUp]  → creates DI container + launches browser process
///   [SetUp]         → creates a fresh BrowserContext + IPage → creates PlaywrightDriver
///   [TearDown]      → disposes context (captures screenshot/trace on failure if configured)
///   [OneTimeTearDown] → closes browser process + DI container
///
/// How to use in a test class:
///   [TestFixture]
///   public class LoginTests : UITestBase
///   {
///       [Test]
///       public async Task Should_LoginSuccessfully()
///       {
///           var loginPage = new LoginPage(Driver, Settings.UiBaseUrl);
///           await loginPage.OpenAsync();
///           await loginPage.LoginAsync("user", "pass");
///           // assert ...
///       }
///   }
/// </summary>
[AllureNUnit]
[Parallelizable(ParallelScope.Fixtures)]
public abstract class UITestBase
{
    private ServiceProvider _serviceProvider = null!;
    private BrowserSession _session = null!;

    protected PlaywrightDriver Driver { get; private set; } = null!;
    protected AutomationSettings Settings { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual Task OneTimeSetUpAsync()
    {
        _serviceProvider = (ServiceProvider)TestServiceProvider.Create();
        Settings = _serviceProvider.GetRequiredService<IOptions<AutomationSettings>>().Value;

        WriteAllureEnvironment();
        return Task.CompletedTask;
    }

    [SetUp]
    public virtual async Task SetUpAsync()
    {
        _session = _serviceProvider.GetRequiredService<BrowserSession>();
        await _session.InitializeAsync();

        var logger = _serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PlaywrightDriver>>();
        Driver = new PlaywrightDriver(_session.Page, Settings.Browser, logger);
    }

    [TearDown]
    public virtual async Task TearDownAsync()
    {
        // Guard: SetUp may have failed before Driver or _session were assigned
        if (Driver is not null
            && Settings?.Browser.ScreenshotOnFailure == true
            && TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            await Driver.AttachScreenshotToReportAsync($"FAILED: {TestContext.CurrentContext.Test.Name}");
        }

        if (_session is not null)
            await _session.DisposeAsync();
    }

    [OneTimeTearDown]
    public virtual async Task OneTimeTearDownAsync()
    {
        if (_session is not null)
            await _session.DisposeBrowserAsync();

        // ServiceProvider implements IAsyncDisposable; must use DisposeAsync because
        // BrowserSession only implements IAsyncDisposable (not IDisposable).
        await _serviceProvider.DisposeAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void WriteAllureEnvironment()
    {
        var props = new Dictionary<string, string>
        {
            ["Environment"]  = Settings.Environment,
            ["UI.BaseUrl"]   = Settings.UiBaseUrl,
            ["Browser.Type"] = Settings.Browser.BrowserType,
            ["Browser.Mode"] = Settings.Browser.Headless ? "Headless" : "Headed"
        };

        AllureHelper.WriteEnvironmentProperties(Settings.Report.AllureResultsDirectory, props);
    }
}
