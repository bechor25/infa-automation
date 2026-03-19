namespace Company.Automation.Configuration.Models;

public sealed class BrowserSettings
{
    /// <summary>chromium | firefox | webkit</summary>
    public string BrowserType { get; init; } = "chromium";

    public bool Headless { get; init; } = true;

    /// <summary>Default timeout in milliseconds for Playwright operations.</summary>
    public float DefaultTimeoutMs { get; init; } = 30_000;

    /// <summary>Number of Polly retries for transient UI failures.</summary>
    public int RetryCount { get; init; } = 2;

    /// <summary>Delay between retries in milliseconds.</summary>
    public int RetryDelayMs { get; init; } = 500;

    /// <summary>Capture screenshot on every test failure.</summary>
    public bool ScreenshotOnFailure { get; init; } = true;

    /// <summary>Capture screenshot on every successful action (verbose — off by default).</summary>
    public bool ScreenshotOnSuccess { get; init; } = false;

    /// <summary>Record Playwright trace archive (trace.zip) on failure.</summary>
    public bool TraceOnFailure { get; init; } = false;

    /// <summary>Record video on failure.</summary>
    public bool VideoOnFailure { get; init; } = false;

    /// <summary>Viewport width.</summary>
    public int ViewportWidth { get; init; } = 1920;

    /// <summary>Viewport height.</summary>
    public int ViewportHeight { get; init; } = 1080;

    /// <summary>Accept all downloads automatically.</summary>
    public bool AcceptDownloads { get; init; } = true;

    /// <summary>Ignore HTTPS certificate errors (useful for test environments).</summary>
    public bool IgnoreHttpsErrors { get; init; } = false;

    /// <summary>Browser launch slow-motion delay in ms (useful for debugging).</summary>
    public int SlowMoMs { get; init; } = 0;
}
