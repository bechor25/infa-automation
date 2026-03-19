namespace Company.Automation.Contracts.UI;

/// <summary>
/// Defines the public surface area for all wrapped Playwright UI actions.
/// Concrete implementation lives in Company.Automation.UI.
/// </summary>
public interface IPageDriver
{
    // ── Navigation ──────────────────────────────────────────────────────────
    Task NavigateAsync(string url);
    Task WaitForUrlAsync(string urlPattern);
    Task WaitForPageLoadAsync(string state = "networkidle");

    // ── Clicks ───────────────────────────────────────────────────────────────
    Task ClickAsync(string selector);
    Task DoubleClickAsync(string selector);
    Task RightClickAsync(string selector);
    Task HoverAsync(string selector);
    Task ClickIfVisibleAsync(string selector);

    // ── Input ────────────────────────────────────────────────────────────────
    Task FillAsync(string selector, string value);
    Task ClearAndFillAsync(string selector, string value);
    Task ClearAsync(string selector);
    Task PressKeyAsync(string selector, string key);
    Task TypeSlowlyAsync(string selector, string text, int delayMs = 50);

    // ── Selection ────────────────────────────────────────────────────────────
    Task SelectByValueAsync(string selector, string value);
    Task SelectByLabelAsync(string selector, string label);
    Task SelectByIndexAsync(string selector, int index);

    // ── Checkboxes / Radio ───────────────────────────────────────────────────
    Task CheckAsync(string selector);
    Task UncheckAsync(string selector);
    Task SetCheckedAsync(string selector, bool state);

    // ── Files / Drag ─────────────────────────────────────────────────────────
    Task UploadFileAsync(string selector, string filePath);
    Task DragAndDropAsync(string sourceSelector, string targetSelector);

    // ── Reading ───────────────────────────────────────────────────────────────
    Task<string> GetTextAsync(string selector);
    Task<string> GetInputValueAsync(string selector);
    Task<string?> GetAttributeAsync(string selector, string attribute);
    /// <summary>
    /// Returns the visible text of the currently selected option in a &lt;select&gt; element.
    /// Uses JavaScript evaluation — the only reliable approach because &lt;option&gt; elements
    /// are inherently hidden in Playwright's visibility model.
    /// </summary>
    Task<string> GetSelectedLabelAsync(string selector);

    // ── State queries ─────────────────────────────────────────────────────────
    Task<bool> IsVisibleAsync(string selector);
    Task<bool> IsHiddenAsync(string selector);
    Task<bool> IsEnabledAsync(string selector);
    Task<bool> IsDisabledAsync(string selector);
    Task<bool> IsCheckedAsync(string selector);

    // ── Waits ─────────────────────────────────────────────────────────────────
    Task WaitForVisibleAsync(string selector, float? timeoutMs = null);
    Task WaitForHiddenAsync(string selector, float? timeoutMs = null);
    Task WaitForEnabledAsync(string selector, float? timeoutMs = null);

    // ── Scrolling ─────────────────────────────────────────────────────────────
    Task ScrollToElementAsync(string selector);
    Task ScrollToTopAsync();
    Task ScrollToBottomAsync();

    // ── Keyboard / Mouse ──────────────────────────────────────────────────────
    Task PressGlobalKeyAsync(string key);
    Task MouseMoveToAsync(string selector);

    // ── Dialogs ───────────────────────────────────────────────────────────────
    Task AcceptDialogAsync(string? promptText = null);
    Task DismissDialogAsync();

    // ── Frames ────────────────────────────────────────────────────────────────
    Task<IPageDriver> SwitchToFrameAsync(string frameSelector);

    // ── Multi-tab ─────────────────────────────────────────────────────────────
    Task<IPageDriver> SwitchToNewTabAsync(Func<Task> triggerAction);

    // ── Evidence ──────────────────────────────────────────────────────────────
    Task<byte[]> TakeScreenshotAsync(string? name = null);
    Task AttachScreenshotToReportAsync(string name);
}
