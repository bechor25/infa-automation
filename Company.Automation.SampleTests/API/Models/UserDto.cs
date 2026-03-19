using System.Text.Json.Serialization;

namespace Company.Automation.SampleTests.API.Models;

// ── Users (/users) ──────────────────────────────────────────────────────────

public sealed record UserDto(
    [property: JsonPropertyName("id")]       int    Id,
    [property: JsonPropertyName("name")]     string Name,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")]    string Email
);

// ── Posts (/posts) ──────────────────────────────────────────────────────────

public sealed record CreatePostRequest(
    [property: JsonPropertyName("title")]  string Title,
    [property: JsonPropertyName("body")]   string Body,
    [property: JsonPropertyName("userId")] int    UserId
);

public sealed record CreatePostResponse(
    [property: JsonPropertyName("id")]     int    Id,
    [property: JsonPropertyName("title")]  string Title,
    [property: JsonPropertyName("body")]   string Body,
    [property: JsonPropertyName("userId")] int    UserId
);
