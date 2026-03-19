# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Repository Is

A production-grade, multi-project C#/.NET 8 automation framework for UI (Playwright) and API testing. It is intended to be published as an internal NuGet package set so that manual testers can write tests with minimal coding effort.

## Solution Structure

```
src/
  Company.Automation.Contracts/      # Interfaces only — no implementations
  Company.Automation.Configuration/  # Strongly-typed settings + DI registration
  Company.Automation.Core/           # Exceptions, resilience (Polly 8), Serilog logging
  Company.Automation.Reporting/      # Allure step/attachment helpers (static AllureHelper)
  Company.Automation.UI/             # Playwright wrapper (PlaywrightDriver, BrowserSession, BasePage)
  Company.Automation.API/            # HttpClient wrapper (ApiClient, ApiRequestBuilder)
  Company.Automation.TestHost/       # NUnit 4 base test classes + DI bootstrap (TestServiceProvider)

samples/
  Company.Automation.SampleTests/    # Reference consumer project with real tests

docker/Dockerfile
build/pack.sh
.github/workflows/ci.yml
```

Dependency flow: `SampleTests → TestHost → UI/API → Reporting → Core → Contracts`

## Common Commands

```bash
# Restore all projects
dotnet restore

# Build entire solution
dotnet build --configuration Release

# Run all sample tests
dotnet test samples/Company.Automation.SampleTests/ --configuration Release

# Run a single test class
dotnet test samples/Company.Automation.SampleTests/ --filter "FullyQualifiedName~LoginTests"

# Run tests by category/story
dotnet test samples/Company.Automation.SampleTests/ --filter "TestCategory=Smoke"

# Install Playwright browsers (required once after dotnet build)
pwsh samples/Company.Automation.SampleTests/bin/Release/net8.0/playwright.ps1 install chromium

# Pack all NuGet packages
./build/pack.sh 1.0.0

# Pack and push to internal feed
NUGET_API_KEY=xxx ./build/pack.sh 1.0.0 https://your-feed/nuget/v3/index.json

# Run tests in Docker (headless, QA environment)
docker build -f docker/Dockerfile -t company-tests .
docker run -v $(pwd)/allure-results:/tests/allure-results company-tests

# Generate Allure HTML report (requires allure CLI)
allure generate allure-results --clean -o allure-report && allure open allure-report
```

## Key Architecture Decisions

- **Test framework: NUnit 4** (not xUnit). NUnit's `[OneTimeSetUp/TearDown]` and `[SetUp/TearDown]` map directly to the three-level Playwright lifecycle (Browser → Context → Page).
- **JSON: System.Text.Json** (not Newtonsoft). Native .NET 8, zero extra dependency.
- **HTTP: typed HttpClient** wrapper (not RestSharp). DI-native, full control over Allure hooks.
- **Resilience: Polly 8** — `ResiliencePipeline` API (not the old `Policy.Handle` API).
- **Logging: Serilog** bridged via `Serilog.Extensions.Logging` to `ILogger<T>`.

## Browser Lifecycle

One `IBrowser` per test fixture class (`[OneTimeSetUp]`), one `IBrowserContext + IPage` per test (`[SetUp]`). This balances isolation (fresh cookies/storage per test) with performance (no browser restart between tests).

## How PlaywrightDriver Works

Every UI action goes through `ExecuteAsync(description, action)` which:
1. Opens an Allure step
2. Runs the action inside a `ResiliencePipeline` (Polly retry on `TimeoutException` / `PlaywrightException`)
3. On failure: captures a screenshot → attaches to Allure → throws `UIActionException`
4. On success: optionally attaches a screenshot (if `ScreenshotOnSuccess` is enabled)

Never call Playwright's `IPage` directly in tests — always use `PlaywrightDriver` or a `BasePage` subclass.

## How ApiClient Works

Every HTTP call goes through `SendAsync` which:
1. Opens an Allure step named `{METHOD} {path}`
2. Serializes the request body with `System.Text.Json`
3. Retries on `HttpRequestException` / `TaskCanceledException` via Polly
4. Attaches request + response to Allure as text attachments
5. Returns a typed `IApiResponse<T>`

## Environment Configuration

Set the `ASPNETCORE_ENVIRONMENT` variable to load the matching `appsettings.{env}.json`:
- `Development` (default, headed browser)
- `QA` (headless, trace on failure)
- `Production` (headless, all diagnostics on)

Individual settings can be overridden with `AUTOMATION_` prefixed env vars:
```
AUTOMATION_Browser__Headless=true
AUTOMATION_Api__BaseUrl=https://custom.api.example.com
```

## Writing a New Test (Tester Guide)

**UI test:**
```csharp
[TestFixture]
public class MyTests : UITestBase
{
    [Test]
    public async Task MyScenario()
    {
        var page = new MyPage(Driver, Settings.UiBaseUrl);
        await page.OpenAsync();
        await page.DoSomethingAsync();
        // assert with FluentAssertions
    }
}
```

**API test:**
```csharp
[TestFixture]
public class MyApiTests : ApiTestBase
{
    [Test]
    public async Task MyApiScenario()
    {
        var response = await Api.GetAsync<MyDto>("/api/resource");
        response.EnsureSuccessStatusCode();
        response.Data.Should().NotBeNull();
    }
}
```

## Writing a New Page Object

1. Create a class that extends `BasePage`
2. Define locators as `private const string` fields
3. Override `GetRelativePath()` to return the page's URL path
4. Override `WaitForPageReadyAsync()` to wait for a landmark element
5. Expose only business-level methods (not individual clicks/fills)

## NuGet Package Strategy

Projects marked `<IsPackable>true</IsPackable>`:
- `Company.Automation.Contracts`
- `Company.Automation.Configuration`
- `Company.Automation.Core`
- `Company.Automation.Reporting`
- `Company.Automation.UI`
- `Company.Automation.API`
- `Company.Automation.TestHost`

`Company.Automation.SampleTests` is **not** packaged — it is a reference consumer.

Consumer test projects should reference only `Company.Automation.TestHost` (it transitively brings everything else).

## Important Notes

- Always run `playwright.ps1 install chromium` after any Playwright version update.
- Polly 8 uses `ResiliencePipelineBuilder` — do not use the old `Policy.Handle` API.
- `AllureHelper` is a static utility class; it does not need to be injected.
- `BrowserSession.DisposeBrowserAsync()` must be called from `[OneTimeTearDown]` to close the browser process; `DisposeAsync()` only closes the context.
- The `PlaywrightDriver.UnderlyingPage` property exposes the raw `IPage` for advanced scenarios not covered by the wrapper — use sparingly.
