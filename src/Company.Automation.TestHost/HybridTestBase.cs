using Allure.NUnit;
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
/// Base class for hybrid test fixtures that combine both UI and API automation.
/// Common use case: use the API to set up test data, then verify via UI; or
/// use the UI to complete a flow and validate the result via API.
///
/// Lifecycle is the same as <see cref="UITestBase"/> with the addition of <see cref="Api"/>.
/// </summary>
[AllureNUnit]
[Parallelizable(ParallelScope.Fixtures)]
public abstract class HybridTestBase
{
    private ServiceProvider _serviceProvider = null!;
    private BrowserSession _session = null!;

    protected PlaywrightDriver Driver { get; private set; } = null!;
    protected IApiClient Api { get; private set; } = null!;
    protected AutomationSettings Settings { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual Task OneTimeSetUpAsync()
    {
        _serviceProvider = (ServiceProvider)TestServiceProvider.Create();
        Settings = _serviceProvider.GetRequiredService<IOptions<AutomationSettings>>().Value;
        Api = _serviceProvider.GetRequiredService<IApiClient>();
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

        await _serviceProvider.DisposeAsync();
    }
}
