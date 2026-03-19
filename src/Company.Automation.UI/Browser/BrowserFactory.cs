using Company.Automation.Configuration.Models;
using Microsoft.Playwright;

namespace Company.Automation.UI.Browser;

/// <summary>
/// Creates and configures Playwright browser instances from <see cref="BrowserSettings"/>.
/// </summary>
internal static class BrowserFactory
{
    public static async Task<IBrowser> LaunchAsync(IPlaywright playwright, BrowserSettings settings)
    {
        var options = new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless,
            SlowMo = settings.SlowMoMs > 0 ? settings.SlowMoMs : null
        };

        return settings.BrowserType.ToLowerInvariant() switch
        {
            "firefox"  => await playwright.Firefox.LaunchAsync(options),
            "webkit"   => await playwright.Webkit.LaunchAsync(options),
            _          => await playwright.Chromium.LaunchAsync(options)
        };
    }

    public static BrowserNewContextOptions BuildContextOptions(BrowserSettings settings)
    {
        var opts = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = settings.ViewportWidth,
                Height = settings.ViewportHeight
            },
            AcceptDownloads = settings.AcceptDownloads,
            IgnoreHTTPSErrors = settings.IgnoreHttpsErrors
        };

        if (settings.VideoOnFailure)
        {
            opts.RecordVideoDir = "videos";
            opts.RecordVideoSize = new RecordVideoSize
            {
                Width = settings.ViewportWidth,
                Height = settings.ViewportHeight
            };
        }

        return opts;
    }
}
