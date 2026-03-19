using Allure.NUnit.Attributes;
using Company.Automation.SampleTests.API.Models;
using Company.Automation.TestHost;
using FluentAssertions;
using NUnit.Framework;
using System.Net;

namespace Company.Automation.SampleTests.API.Tests;

/// <summary>
/// Demonstrates API test patterns using the framework against jsonplaceholder.typicode.com.
///
/// The tester:
///   1. Inherits ApiTestBase (handles DI, Allure, configuration)
///   2. Calls Api.GetAsync / PostAsync with model types
///   3. Calls EnsureSuccessStatusCode() or checks StatusCode directly
///   4. Asserts on the typed Data property
///
/// The framework handles: request logging, response logging, Allure attachment, retry.
/// </summary>
[TestFixture]
[AllureSuite("Users API")]
[AllureFeature("User Management")]
public class UserApiTests : ApiTestBase
{
    [Test]
    [AllureStory("Retrieve a list of users")]
    public async Task GetUsers_ShouldReturnList()
    {
        var response = await Api.GetAsync<List<UserDto>>("/users");

        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCountGreaterThan(0, "the endpoint should return users");
    }

    [Test]
    [AllureStory("Retrieve a single user by ID")]
    public async Task GetUser_WithValidId_ShouldReturnUserDetails()
    {
        var response = await Api.GetAsync<UserDto>("/users/1");

        response.EnsureSuccessStatusCode();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(1);
        response.Data.Email.Should().NotBeNullOrEmpty();
        response.Data.Name.Should().NotBeNullOrEmpty();
    }

    [Test]
    [AllureStory("Retrieve a non-existent user returns 404")]
    public async Task GetUser_WithInvalidId_ShouldReturn404()
    {
        var response = await Api.GetAsync<object>("/users/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "requesting a non-existent resource should return 404");
        response.IsSuccessful.Should().BeFalse();
    }

    [Test]
    [AllureStory("Create a new post")]
    public async Task CreatePost_WithValidPayload_ShouldReturn201()
    {
        var newPost = new CreatePostRequest("Automation test post", "Created by the framework", UserId: 1);

        var response = await Api.PostAsync<CreatePostResponse>("/posts", newPost);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Data.Should().NotBeNull();
        response.Data!.Title.Should().Be("Automation test post");
        response.Data.UserId.Should().Be(1);
        response.Data.Id.Should().BeGreaterThan(0, "server should assign an ID");
    }

    [Test]
    [AllureStory("Use fluent request builder for complex requests")]
    public async Task GetUsers_UsingRequestBuilder_ShouldSucceed()
    {
        var response = await Api.NewRequest("/users")
            .WithHeader("X-Custom-Header", "automation-test")
            .GetAsync<List<UserDto>>();

        response.EnsureSuccessStatusCode();
        response.Data.Should().NotBeNullOrEmpty();
    }
}
