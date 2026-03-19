using Allure.NUnit.Attributes;
using Company.Automation.SampleTests.API.Models;
using Company.Automation.SampleTests.UI.Pages;
using Company.Automation.TestHost;
using FluentAssertions;
using NUnit.Framework;

namespace Company.Automation.SampleTests.Hybrid.Tests;

/// <summary>
/// Demonstrates combining API setup with UI verification in a single test.
///
/// Pattern: Use the API to set up or verify state; use the UI for end-to-end validation.
/// This keeps test data predictable while exercising the full user journey.
/// </summary>
[TestFixture]
[AllureSuite("Hybrid Tests")]
[AllureFeature("API + UI")]
public class HybridExampleTests : HybridTestBase
{
    [Test]
    [AllureStory("API confirms user exists, UI login works")]
    public async Task UserExistsViaApi_ShouldBeAbleToLoginViaUI()
    {
        // Step 1: Verify the user exists via API
        var apiResponse = await Api.GetAsync<UserDto>("/users/1");
        apiResponse.EnsureSuccessStatusCode();
        apiResponse.Data.Should().NotBeNull("user must exist before UI login test");
        apiResponse.Data!.Email.Should().NotBeNullOrEmpty("user should have an email address");

        // Step 2: Use UI to complete the login flow
        var loginPage = new LoginPage(Driver, Settings.UiBaseUrl);
        var dashboardPage = new DashboardPage(Driver, Settings.UiBaseUrl);

        await loginPage.OpenAsync();
        await loginPage.LoginAsync("tomsmith", "SuperSecretPassword!");
        await dashboardPage.WaitForPageReadyAsync();

        var isLoggedIn = await dashboardPage.IsLoggedInAsync();
        isLoggedIn.Should().BeTrue();
    }
}
