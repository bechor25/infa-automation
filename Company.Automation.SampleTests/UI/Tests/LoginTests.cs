using Allure.NUnit.Attributes;
using Company.Automation.SampleTests.UI.Pages;
using Company.Automation.TestHost;
using FluentAssertions;
using NUnit.Framework;

namespace Company.Automation.SampleTests.UI.Tests;

/// <summary>
/// Demonstrates a consumer UI test using the framework.
///
/// The tester:
///   1. Inherits UITestBase (handles all browser lifecycle, DI, Allure)
///   2. Creates page objects with Driver and Settings.UiBaseUrl
///   3. Calls business-level methods on the page objects
///   4. Asserts with FluentAssertions
///
/// No Playwright API is visible in the test — the framework hides it completely.
/// </summary>
[TestFixture]
[AllureSuite("Authentication")]
[AllureFeature("Login")]
public class LoginTests : UITestBase
{
    [Test]
    [AllureStory("Valid credentials grant access")]
    [AllureSeverity(Allure.Net.Commons.SeverityLevel.critical)]
    public async Task Login_WithValidCredentials_ShouldNavigateToDashboard()
    {
        // Arrange
        var loginPage = new LoginPage(Driver, Settings.UiBaseUrl);
        var dashboardPage = new DashboardPage(Driver, Settings.UiBaseUrl);

        // Act
        await loginPage.OpenAsync();
        await loginPage.WaitForPageReadyAsync();
        await loginPage.LoginAsync("tomsmith", "SuperSecretPassword!");

        // Assert
        await dashboardPage.WaitForPageReadyAsync();
        var isLoggedIn = await dashboardPage.IsLoggedInAsync();
        isLoggedIn.Should().BeTrue("user should be redirected to dashboard after valid login");
    }

    [Test]
    [AllureStory("Invalid credentials are rejected")]
    [AllureSeverity(Allure.Net.Commons.SeverityLevel.critical)]
    public async Task Login_WithInvalidCredentials_ShouldShowErrorMessage()
    {
        // Arrange
        var loginPage = new LoginPage(Driver, Settings.UiBaseUrl);

        // Act
        await loginPage.OpenAsync();
        await loginPage.WaitForPageReadyAsync();
        await loginPage.LoginAsync("wronguser", "wrongpassword");

        // Assert
        var isErrorShown = await loginPage.IsErrorDisplayedAsync();
        isErrorShown.Should().BeTrue("error message should be displayed for invalid credentials");

        var errorText = await loginPage.GetErrorMessageAsync();
        errorText.Should().Contain("Your username is invalid", "error should indicate the problem clearly");
    }

    [Test]
    [AllureStory("Logout clears session")]
    public async Task Login_ThenLogout_ShouldReturnToLoginPage()
    {
        // Arrange
        var loginPage = new LoginPage(Driver, Settings.UiBaseUrl);
        var dashboardPage = new DashboardPage(Driver, Settings.UiBaseUrl);

        // Act
        await loginPage.OpenAsync();
        await loginPage.LoginAsync("tomsmith", "SuperSecretPassword!");
        await dashboardPage.WaitForPageReadyAsync();
        await dashboardPage.LogoutAsync();

        // Assert
        await loginPage.WaitForPageReadyAsync();
        var isBackOnLogin = await Driver.IsVisibleAsync("#username");
        isBackOnLogin.Should().BeTrue("user should be back on the login page after logout");
    }
}
