using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.UI;
using Company.Automation.Core.Exceptions;
using Company.Automation.Core.Resilience;
using Company.Automation.Reporting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Polly;

namespace Company.Automation.UI.Driver;

/// <summary>
/// The primary UI automation driver. Wraps every Playwright action with:
///   1. Allure step reporting
///   2. Polly retry on transient failures
///   3. Structured logging
///   4. Screenshot on failure (attached to Allure)
///   5. Domain-specific exception with human-readable message
///
/// Testers interact with this class via page objects — they never touch IPage directly.
/// </summary>
public sealed class PlaywrightDriver : IPageDriver
{
    private readonly IPage _page;
    private readonly BrowserSettings _settings;
    private readonly ILogger<PlaywrightDriver> _logger;
    private readonly ResiliencePipeline _pipeline;

    public PlaywrightDriver(IPage page, BrowserSettings settings, ILogger<PlaywrightDriver> logger)
    {
        _page = page;
        _settings = settings;
        _logger = logger;
        _pipeline = ResiliencePolicy.BuildRetryPipeline(
            settings.RetryCount,
            settings.RetryDelayMs,
            ex => ex is TimeoutException ||
                  (ex is PlaywrightException pe && (
                      pe.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ||
                      pe.Message.Contains("waiting for", StringComparison.OrdinalIgnoreCase) ||
                      pe.Message.Contains("detached", StringComparison.OrdinalIgnoreCase))));

        // Set Playwright's internal timeout to match configuration
        _page.SetDefaultTimeout(settings.DefaultTimeoutMs);
        _page.SetDefaultNavigationTimeout(settings.DefaultTimeoutMs);
    }

    // ── Core execution wrappers ──────────────────────────────────────────────

    /// <summary>Executes a UI action inside an Allure step with retry and failure evidence.</summary>
    private async Task ExecuteAsync(string description, Func<Task> action)
    {
        _logger.LogDebug("[UI] {Action}", description);

        await AllureHelper.StepAsync(description, async () =>
        {
            try
            {
                await _pipeline.ExecuteAsync(async _ => await action());
                _logger.LogDebug("[UI] OK: {Action}", description);

                if (_settings.ScreenshotOnSuccess)
                    await AttachScreenshotAsync($"✓ {description}");
            }
            catch (Exception ex) when (ex is not UIActionException)
            {
                _logger.LogError(ex, "[UI] FAIL: {Action}", description);

                if (_settings.ScreenshotOnFailure)
                    await AttachScreenshotAsync($"✗ {description}");

                throw new UIActionException(description, ex);
            }
        });
    }

    /// <summary>Executes a UI action that returns a value, inside an Allure step.</summary>
    private async Task<T> ExecuteAsync<T>(string description, Func<Task<T>> action)
    {
        T result = default!;
        await ExecuteAsync(description, async () => { result = await action(); });
        return result;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public async Task NavigateAsync(string url) =>
        await ExecuteAsync($"Navigate → {url}", () =>
            _page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = _settings.DefaultTimeoutMs
            }));

    public async Task WaitForUrlAsync(string urlPattern) =>
        await ExecuteAsync($"Wait for URL: {urlPattern}", () =>
            _page.WaitForURLAsync(urlPattern, new PageWaitForURLOptions
            {
                Timeout = _settings.DefaultTimeoutMs
            }));

    public async Task WaitForPageLoadAsync(string state = "networkidle") =>
        await ExecuteAsync($"Wait for page load state: {state}", () =>
            _page.WaitForLoadStateAsync(state switch
            {
                "domcontentloaded" => LoadState.DOMContentLoaded,
                "load"             => LoadState.Load,
                _                  => LoadState.NetworkIdle
            }));

    // ── Clicks ────────────────────────────────────────────────────────────────

    public async Task ClickAsync(string selector) =>
        await ExecuteAsync($"Click [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = _settings.DefaultTimeoutMs
            });
            await loc.ClickAsync();
        });

    public async Task DoubleClickAsync(string selector) =>
        await ExecuteAsync($"Double-click [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.DblClickAsync();
        });

    public async Task RightClickAsync(string selector) =>
        await ExecuteAsync($"Right-click [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });
        });

    public async Task HoverAsync(string selector) =>
        await ExecuteAsync($"Hover [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.HoverAsync();
        });

    public async Task ClickIfVisibleAsync(string selector) =>
        await ExecuteAsync($"Click if visible [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            var count = await loc.CountAsync();
            if (count > 0 && await loc.IsVisibleAsync())
                await loc.ClickAsync();
        });

    // ── Input ─────────────────────────────────────────────────────────────────

    public async Task FillAsync(string selector, string value) =>
        await ExecuteAsync($"Fill [{selector}] = \"{value}\"", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.FillAsync(value);
        });

    public async Task ClearAndFillAsync(string selector, string value) =>
        await ExecuteAsync($"Clear+Fill [{selector}] = \"{value}\"", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.ClearAsync();
            await loc.FillAsync(value);
        });

    public async Task ClearAsync(string selector) =>
        await ExecuteAsync($"Clear [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.ClearAsync();
        });

    public async Task PressKeyAsync(string selector, string key) =>
        await ExecuteAsync($"Press key [{key}] on [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.PressAsync(key);
        });

    public async Task TypeSlowlyAsync(string selector, string text, int delayMs = 50) =>
        await ExecuteAsync($"Type slowly [{selector}] = \"{text}\"", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delayMs });
        });

    // ── Selection ─────────────────────────────────────────────────────────────

    public async Task SelectByValueAsync(string selector, string value) =>
        await ExecuteAsync($"Select by value [{selector}] = \"{value}\"", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.SelectOptionAsync(new SelectOptionValue { Value = value });
        });

    public async Task SelectByLabelAsync(string selector, string label) =>
        await ExecuteAsync($"Select by label [{selector}] = \"{label}\"", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.SelectOptionAsync(new SelectOptionValue { Label = label });
        });

    public async Task SelectByIndexAsync(string selector, int index) =>
        await ExecuteAsync($"Select by index [{selector}] = {index}", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.SelectOptionAsync(new SelectOptionValue { Index = index });
        });

    // ── Checkboxes ────────────────────────────────────────────────────────────

    public async Task CheckAsync(string selector) =>
        await ExecuteAsync($"Check [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.CheckAsync();
        });

    public async Task UncheckAsync(string selector) =>
        await ExecuteAsync($"Uncheck [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.UncheckAsync();
        });

    public async Task SetCheckedAsync(string selector, bool state) =>
        await ExecuteAsync($"Set checked={state} [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await loc.SetCheckedAsync(state);
        });

    // ── Files / Drag ──────────────────────────────────────────────────────────

    public async Task UploadFileAsync(string selector, string filePath) =>
        await ExecuteAsync($"Upload file [{filePath}] via [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.SetInputFilesAsync(filePath);
        });

    public async Task DragAndDropAsync(string sourceSelector, string targetSelector) =>
        await ExecuteAsync($"Drag [{sourceSelector}] → [{targetSelector}]", () =>
            _page.DragAndDropAsync(sourceSelector, targetSelector));

    // ── Reading ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the text content of the element identified by <paramref name="selector"/>.
    /// Waits for the element to be <b>attached</b> (not necessarily visible), so it works
    /// correctly on inherently-hidden elements such as <c>&lt;option&gt;</c> inside a
    /// <c>&lt;select&gt;</c>.  Uses <c>TextContentAsync</c> (DOM-based, visibility-agnostic)
    /// rather than <c>InnerTextAsync</c> (CSS-visibility-aware).
    /// </summary>
    public async Task<string> GetTextAsync(string selector) =>
        await ExecuteAsync($"Get text [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
            return await loc.TextContentAsync() ?? string.Empty;
        });

    /// <summary>
    /// Returns the visible text of the currently selected option in a &lt;select&gt; element.
    /// Evaluates <c>el.options[el.selectedIndex].text</c> via JavaScript because
    /// Playwright's locator API considers &lt;option&gt; elements hidden and will time out
    /// if you attempt to wait for or read them through normal visibility-based methods.
    /// </summary>
    public async Task<string> GetSelectedLabelAsync(string selector) =>
        await ExecuteAsync($"Get selected label [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions
            {
                State   = WaitForSelectorState.Visible,
                Timeout = _settings.DefaultTimeoutMs
            });
            return await loc.EvaluateAsync<string>(
                "el => el.options[el.selectedIndex]?.text?.trim() ?? ''");
        });

    public async Task<string> GetInputValueAsync(string selector) =>
        await ExecuteAsync($"Get input value [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return await loc.InputValueAsync() ?? string.Empty;
        });

    public async Task<string?> GetAttributeAsync(string selector, string attribute) =>
        await ExecuteAsync($"Get attribute [{attribute}] of [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
            return await loc.GetAttributeAsync(attribute);
        });

    // ── State queries ─────────────────────────────────────────────────────────

    public async Task<bool> IsVisibleAsync(string selector) =>
        await ExecuteAsync($"Is visible? [{selector}]", () =>
            _page.Locator(selector).IsVisibleAsync());

    public async Task<bool> IsHiddenAsync(string selector) =>
        await ExecuteAsync($"Is hidden? [{selector}]", () =>
            _page.Locator(selector).IsHiddenAsync());

    public async Task<bool> IsEnabledAsync(string selector) =>
        await ExecuteAsync($"Is enabled? [{selector}]", () =>
            _page.Locator(selector).IsEnabledAsync());

    public async Task<bool> IsDisabledAsync(string selector) =>
        await ExecuteAsync($"Is disabled? [{selector}]", () =>
            _page.Locator(selector).IsDisabledAsync());

    public async Task<bool> IsCheckedAsync(string selector) =>
        await ExecuteAsync($"Is checked? [{selector}]", () =>
            _page.Locator(selector).IsCheckedAsync());

    // ── Waits ─────────────────────────────────────────────────────────────────

    public async Task WaitForVisibleAsync(string selector, float? timeoutMs = null) =>
        await ExecuteAsync($"Wait for visible [{selector}]", () =>
            _page.Locator(selector).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs ?? _settings.DefaultTimeoutMs
            }));

    public async Task WaitForHiddenAsync(string selector, float? timeoutMs = null) =>
        await ExecuteAsync($"Wait for hidden [{selector}]", () =>
            _page.Locator(selector).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = timeoutMs ?? _settings.DefaultTimeoutMs
            }));

    public async Task WaitForEnabledAsync(string selector, float? timeoutMs = null) =>
        await ExecuteAsync($"Wait for enabled [{selector}]", async () =>
        {
            var loc = _page.Locator(selector);
            await loc.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs ?? _settings.DefaultTimeoutMs
            });
            // Playwright doesn't have a built-in "wait for enabled" state on locator,
            // so we poll with a short loop.
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs ?? _settings.DefaultTimeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (await loc.IsEnabledAsync()) return;
                await Task.Delay(100);
            }
            throw new ElementNotFoundException(selector, "enabled");
        });

    // ── Scrolling ─────────────────────────────────────────────────────────────

    public async Task ScrollToElementAsync(string selector) =>
        await ExecuteAsync($"Scroll to [{selector}]", () =>
            _page.Locator(selector).ScrollIntoViewIfNeededAsync());

    public async Task ScrollToTopAsync() =>
        await ExecuteAsync("Scroll to top", () =>
            _page.EvaluateAsync("window.scrollTo(0, 0)"));

    public async Task ScrollToBottomAsync() =>
        await ExecuteAsync("Scroll to bottom", () =>
            _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)"));

    // ── Keyboard / Mouse ──────────────────────────────────────────────────────

    public async Task PressGlobalKeyAsync(string key) =>
        await ExecuteAsync($"Press key [{key}]", () => _page.Keyboard.PressAsync(key));

    public async Task MouseMoveToAsync(string selector) =>
        await ExecuteAsync($"Mouse move to [{selector}]", async () =>
        {
            var box = await _page.Locator(selector).BoundingBoxAsync();
            if (box is null) throw new ElementNotFoundException(selector, "visible for mouse move");
            await _page.Mouse.MoveAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
        });

    // ── Dialogs ───────────────────────────────────────────────────────────────

    public async Task AcceptDialogAsync(string? promptText = null)
    {
        _page.Dialog += async (_, dialog) =>
        {
            _logger.LogDebug("[UI] Accepting dialog: {Message}", dialog.Message);
            await dialog.AcceptAsync(promptText);
        };
        await Task.CompletedTask;
    }

    public async Task DismissDialogAsync()
    {
        _page.Dialog += async (_, dialog) =>
        {
            _logger.LogDebug("[UI] Dismissing dialog: {Message}", dialog.Message);
            await dialog.DismissAsync();
        };
        await Task.CompletedTask;
    }

    // ── Frames ────────────────────────────────────────────────────────────────

    public async Task<IPageDriver> SwitchToFrameAsync(string frameSelector) =>
        await ExecuteAsync($"Switch to frame [{frameSelector}]", async () =>
        {
            var frame = _page.FrameLocator(frameSelector);
            // Return a FrameDriver wrapping the frame's owner page for chained actions.
            // For now, return a new driver scoped to the frame's first page.
            var innerPage = await _page.Context.NewPageAsync();
            return (IPageDriver)new PlaywrightDriver(innerPage, _settings, _logger);
        });

    // ── Multi-tab ─────────────────────────────────────────────────────────────

    public async Task<IPageDriver> SwitchToNewTabAsync(Func<Task> triggerAction) =>
        await ExecuteAsync("Switch to new tab", async () =>
        {
            var newPageTask = _page.Context.WaitForPageAsync();
            await triggerAction();
            var newPage = await newPageTask;
            await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            return (IPageDriver)new PlaywrightDriver(newPage, _settings, _logger);
        });

    // ── Evidence ─────────────────────────────────────────────────────────────

    public async Task<byte[]> TakeScreenshotAsync(string? name = null)
    {
        try
        {
            return await _page.ScreenshotAsync(new PageScreenshotOptions { FullPage = false });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture screenshot '{Name}'", name);
            return Array.Empty<byte>();
        }
    }

    public async Task AttachScreenshotToReportAsync(string name)
    {
        var bytes = await TakeScreenshotAsync(name);
        AllureHelper.AttachScreenshot(name, bytes);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private async Task AttachScreenshotAsync(string label)
    {
        try
        {
            var bytes = await _page.ScreenshotAsync(new PageScreenshotOptions { FullPage = false });
            AllureHelper.AttachScreenshot(label, bytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not capture screenshot");
        }
    }

    /// <summary>Exposes the underlying IPage for advanced scenarios in page objects.</summary>
    public IPage UnderlyingPage => _page;
}
