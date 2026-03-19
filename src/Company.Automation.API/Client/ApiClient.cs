using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Company.Automation.Configuration.Models;
using Company.Automation.Contracts.API;
using Company.Automation.Core.Resilience;
using Company.Automation.Reporting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Company.Automation.API.Client;

/// <summary>
/// The primary API test client. Every request is:
///   1. Wrapped in an Allure step
///   2. Logged (request + response)
///   3. Optionally attached to the Allure report as evidence
///   4. Retried on transient failures via Polly
/// </summary>
public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _settings;
    private readonly ILogger<ApiClient> _logger;
    private readonly ResiliencePipeline _pipeline;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(
        HttpClient httpClient,
        IOptions<ApiSettings> settings,
        ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _pipeline = ResiliencePolicy.BuildApiRetryPipeline(_settings.RetryCount, _settings.RetryDelayMs);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public Task<IApiResponse<T>> GetAsync<T>(
        string path,
        IDictionary<string, string>? queryParams = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Get, path, null, queryParams, headers, ct);

    public Task<IApiResponse<T>> PostAsync<T>(
        string path,
        object? body = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Post, path, body, null, headers, ct);

    public Task<IApiResponse<T>> PutAsync<T>(
        string path,
        object? body = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Put, path, body, null, headers, ct);

    public Task<IApiResponse<T>> PatchAsync<T>(
        string path,
        object? body = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default) =>
        SendAsync<T>(new HttpMethod("PATCH"), path, body, null, headers, ct);

    public async Task<IApiResponse> DeleteAsync(
        string path,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default)
    {
        var response = await SendAsync<object>(HttpMethod.Delete, path, null, null, headers, ct);
        return response;
    }

    public IApiRequestBuilder NewRequest(string path) => new ApiRequestBuilder(this, path);

    // ── Core send ─────────────────────────────────────────────────────────────

    private async Task<IApiResponse<T>> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        IDictionary<string, string>? queryParams,
        IDictionary<string, string>? headers,
        CancellationToken ct)
    {
        var uri = BuildUri(path, queryParams);
        var stepName = $"{method.Method} {uri.PathAndQuery}";

        return await AllureHelper.StepAsync(stepName, async () =>
        {
            HttpResponseMessage httpResponse = null!;
            string rawBody = string.Empty;
            string? requestBodyJson = null;

            await _pipeline.ExecuteAsync(async token =>
            {
                using var request = BuildRequest(method, uri, body, headers);
                requestBodyJson = body is null ? null : JsonSerializer.Serialize(body, JsonOptions);

                _logger.LogDebug("[API] → {Method} {Uri}", method, uri);

                if (_settings.AttachRequestToReport)
                    AllureHelper.AttachApiRequest(
                        method.Method,
                        uri.ToString(),
                        requestBodyJson,
                        headers);

                httpResponse = await _httpClient.SendAsync(request, token);
                rawBody = await httpResponse.Content.ReadAsStringAsync(token);

                _logger.LogDebug("[API] ← {Status} ({Length} bytes)", httpResponse.StatusCode, rawBody.Length);
            }, ct);

            var responseHeaders = httpResponse.Headers
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            if (_settings.AttachResponseToReport)
                AllureHelper.AttachApiResponse((int)httpResponse.StatusCode, rawBody, responseHeaders);

            T? data = default;
            if (!string.IsNullOrWhiteSpace(rawBody) && typeof(T) != typeof(object))
            {
                try
                {
                    data = JsonSerializer.Deserialize<T>(rawBody, JsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "[API] Failed to deserialize response to {Type}", typeof(T).Name);
                }
            }

            return (IApiResponse<T>)new ApiResponse<T>
            {
                StatusCode = httpResponse.StatusCode,
                RawBody = rawBody,
                Data = data,
                Headers = responseHeaders,
                RequestUri = uri,
                HttpMethod = method.Method
            };
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Uri BuildUri(string path, IDictionary<string, string>? queryParams)
    {
        var fullPath = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{_settings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

        if (queryParams is { Count: > 0 })
        {
            var query = string.Join("&", queryParams.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            fullPath = $"{fullPath}?{query}";
        }

        return new Uri(fullPath);
    }

    private static HttpRequestMessage BuildRequest(
        HttpMethod method,
        Uri uri,
        object? body,
        IDictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(method, uri);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
                request.Headers.TryAddWithoutValidation(key, value);
        }

        return request;
    }
}
