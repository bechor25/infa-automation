using Allure.NUnit.Attributes;
using Company.Automation.SampleTests.UI.Pages;
using Company.Automation.TestHost;
using FluentAssertions;
using NUnit.Framework;

namespace Company.Automation.SampleTests.UI.Tests;

/// <summary>
/// Demonstrates form interaction patterns: dropdowns, checkboxes, screenshots.
/// </summary>
[TestFixture]
[AllureSuite("Forms")]
[AllureFeature("Dropdown")]
public class FormTests : UITestBase
{
    [Test]
    [AllureStory("User can select a dropdown option by label")]
    public async Task Dropdown_SelectOption1_ShouldUpdateSelection()
    {
        // Arrange
        var page = new DropdownPage(Driver, Settings.UiBaseUrl);

        // Act
        await page.OpenAsync();
        await page.WaitForPageReadyAsync();
        await page.SelectOptionByTextAsync("Option 1");

        // Assert
        var selected = await page.GetSelectedTextAsync();
        selected.Should().Be("Option 1");

        // Manual screenshot evidence attached to report
        await Driver.AttachScreenshotToReportAsync("Dropdown - Option 1 selected");
    }

    [Test]
    [AllureStory("User can change selection to a different option")]
    public async Task Dropdown_SelectOption2_ShouldUpdateSelection()
    {
        var page = new DropdownPage(Driver, Settings.UiBaseUrl);

        await page.OpenAsync();
        await page.SelectOptionByValueAsync("2");

        var selected = await page.GetSelectedTextAsync();
        selected.Should().Be("Option 2");
    }
}
