using Company.Automation.UI.Driver;

namespace Company.Automation.UI.Pages;

/// <summary>
/// Base class for reusable UI components (navigation bars, modals, data tables, forms, etc.)
/// that can be composed inside multiple page objects.
///
/// Example usage in a page object:
///   public NavigationBar Nav => new(_driver);
///   public DataTable ResultsTable => new(_driver, "#results-table");
/// </summary>
public abstract class BaseComponent
{
    protected PlaywrightDriver Driver { get; }

    /// <summary>
    /// Optional root selector that scopes all locators inside this component.
    /// Prepend this to child selectors: $"{Root} >> .child-element"
    /// </summary>
    protected string? Root { get; }

    protected BaseComponent(PlaywrightDriver driver, string? rootSelector = null)
    {
        Driver = driver;
        Root = rootSelector;
    }

    /// <summary>
    /// Resolves a child selector, optionally scoped under the component root.
    /// Usage: var sel = Scope(".submit-btn");
    /// </summary>
    protected string Scope(string childSelector) =>
        string.IsNullOrEmpty(Root) ? childSelector : $"{Root} >> {childSelector}";

    /// <summary>Waits for the component's root element to become visible.</summary>
    public virtual Task WaitForVisibleAsync() =>
        Root is not null
            ? Driver.WaitForVisibleAsync(Root)
            : Task.CompletedTask;
}
