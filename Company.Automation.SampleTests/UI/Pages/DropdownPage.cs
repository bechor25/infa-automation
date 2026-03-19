using Company.Automation.UI.Driver;
using Company.Automation.UI.Pages;

namespace Company.Automation.SampleTests.UI.Pages;

/// <summary>
/// Page object for https://the-internet.herokuapp.com/dropdown.
/// Demonstrates dropdown selection and reading the currently selected value.
/// </summary>
public sealed class DropdownPage : BasePage
{
    private const string Dropdown = "#dropdown";

    protected override string GetRelativePath() => "/dropdown";

    public DropdownPage(PlaywrightDriver driver, string baseUrl)
        : base(driver, baseUrl) { }

    public override async Task WaitForPageReadyAsync() =>
        await Driver.WaitForVisibleAsync(Dropdown);

    /// <summary>Selects an option by its visible text label.</summary>
    public async Task SelectOptionByTextAsync(string label) =>
        await Driver.SelectByLabelAsync(Dropdown, label);

    /// <summary>Selects an option by its HTML value attribute.</summary>
    public async Task SelectOptionByValueAsync(string value) =>
        await Driver.SelectByValueAsync(Dropdown, value);

    /// <summary>
    /// Returns the text of the currently selected option.
    /// Delegates to <c>GetSelectedLabelAsync</c> which uses JavaScript evaluation
    /// on the &lt;select&gt; element — the only reliable way since Playwright
    /// considers &lt;option&gt; elements hidden and times out on visibility-based reads.
    /// </summary>
    public async Task<string> GetSelectedTextAsync() =>
        await Driver.GetSelectedLabelAsync(Dropdown);
}
