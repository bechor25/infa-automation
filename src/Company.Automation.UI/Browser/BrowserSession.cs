using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace Company.Automation.UI.Browser;

/// <summary>
/// Manages the Playwright object lifecycle for a single test:
///   IPlaywright → IBrowser → IBrowserContext → IPage
///
/// The IBrowser is created once per instance and should be shared across tests in a fixture
/// (create in [OneTimeSetUp], dispose in [OneTimeTearDown]).
/// The IBrowserContext and IPage are per-test (InitializeAsync in [SetUp], DisposeAsync in [TearDown]).
/// </summary>
public sealed class BrowserSession : IBrowserSession
{
    private readonly BrowserSettings _settings;
    private readonly ILogger<BrowserSession> _logger;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private bool _contextInitialized;

    public IPage Page => _page ?? throw new InvalidOperationException(
        "BrowserSession has not been initialized. Call InitializeAsync() first.");

    public IBrowserContext Context => _context ?? throw new InvalidOperationException(
        "BrowserSession has not been initialized. Call InitializeAsync() first.");

    public BrowserSession(IOptions<BrowserSettings> settings, ILogger<BrowserSession> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Starts Playwright, launches the browser, creates a context and an initial page.
    /// Call once per test in [SetUp].
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogDebug("Initializing browser session [{BrowserType}, headless={Headless}]",
            _settings.BrowserType, _settings.Headless);

        _playwright = await Playwright.CreateAsync();
        _browser = await BrowserFactory.LaunchAsync(_playwright, _settings);

        await CreateContextAndPageAsync();

        _logger.LogDebug("Browser session ready");
    }

    /// <summary>Resets the browser context (call between tests in the same fixture to get a fresh state).</summary>
    public async Task ResetContextAsync()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
            _context = null;
            _page = null;
        }
        await CreateContextAndPageAsync();
    }

    private async Task CreateContextAndPageAsync()
    {
        if (_browser is null) throw new InvalidOperationException("Browser has not been launched.");

        var contextOptions = BrowserFactory.BuildContextOptions(_settings);

        if (_settings.TraceOnFailure)
        {
            // Trace must be started before any page action.
            contextOptions.RecordVideoDir = null; // separate from trace
        }

        _context = await _browser.NewContextAsync(contextOptions);
        _context.SetDefaultTimeout(_settings.DefaultTimeoutMs);

        if (_settings.TraceOnFailure)
        {
            await _context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = false
            });
        }

        _page = await _context.NewPageAsync();
        _contextInitialized = true;
    }

    /// <summary>
    /// Saves trace/video artifacts if configured, then disposes context and page.
    /// Does NOT dispose the browser — the fixture is responsible for that.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_contextInitialized) return;

        try
        {
            if (_settings.TraceOnFailure && _context is not null)
            {
                var tracePath = Path.Combine("traces", $"trace_{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
                Directory.CreateDirectory("traces");
                await _context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
                _logger.LogDebug("Trace saved: {Path}", tracePath);
            }

            if (_page is not null) await _page.CloseAsync();
            if (_context is not null) await _context.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during BrowserSession dispose");
        }
        finally
        {
            _page = null;
            _context = null;
            _contextInitialized = false;
        }
    }

    /// <summary>
    /// Disposes the underlying browser and Playwright instance.
    /// Call from [OneTimeTearDown] in the test fixture.
    /// </summary>
    public async Task DisposeBrowserAsync()
    {
        await DisposeAsync();

        try
        {
            if (_browser is not null) await _browser.CloseAsync();
            _playwright?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing browser");
        }
        finally
        {
            _browser = null;
            _playwright = null;
        }
    }
}
