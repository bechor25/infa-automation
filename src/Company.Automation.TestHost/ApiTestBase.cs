using Allure.NUnit;
using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Company.Automation.TestHost;

/// <summary>
/// Base class for API-only test fixtures.
/// Provides a fully configured <see cref="IApiClient"/> and <see cref="AutomationSettings"/>.
///
/// How to use:
///   [TestFixture]
///   public class UserApiTests : ApiTestBase
///   {
///       [Test]
///       public async Task GetUser_ShouldReturn200()
///       {
///           var response = await Api.GetAsync&lt;UserDto&gt;("/api/users/1");
///           response.EnsureSuccessStatusCode();
///           response.Data.Should().NotBeNull();
///       }
///   }
/// </summary>
[AllureNUnit]
[Parallelizable(ParallelScope.Fixtures)]
public abstract class ApiTestBase
{
    private ServiceProvider _serviceProvider = null!;

    protected IApiClient Api { get; private set; } = null!;
    protected AutomationSettings Settings { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual Task OneTimeSetUpAsync()
    {
        _serviceProvider = (ServiceProvider)TestServiceProvider.Create();
        Settings = _serviceProvider.GetRequiredService<IOptions<AutomationSettings>>().Value;
        Api = _serviceProvider.GetRequiredService<IApiClient>();
        return Task.CompletedTask;
    }

    [OneTimeTearDown]
    public virtual async Task OneTimeTearDownAsync()
    {
        await _serviceProvider.DisposeAsync();
    }
}
