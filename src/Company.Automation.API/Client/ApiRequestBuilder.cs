using System.Text;
using Company.Automation.Contracts.API;

namespace Company.Automation.API.Client;

/// <summary>
/// Fluent builder for composing complex API requests.
/// Obtained via <see cref="ApiClient.NewRequest"/>.
/// </summary>
internal sealed class ApiRequestBuilder : IApiRequestBuilder
{
    private readonly ApiClient _client;
    private readonly string _path;
    private readonly Dictionary<string, string> _headers = new();
    private readonly Dictionary<string, string> _queryParams = new();
    private object? _body;
    private TimeSpan? _timeout;

    internal ApiRequestBuilder(ApiClient client, string path)
    {
        _client = client;
        _path = path;
    }

    public IApiRequestBuilder WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    public IApiRequestBuilder WithHeaders(IDictionary<string, string> headers)
    {
        foreach (var (k, v) in headers) _headers[k] = v;
        return this;
    }

    public IApiRequestBuilder WithQueryParam(string name, string value)
    {
        _queryParams[name] = value;
        return this;
    }

    public IApiRequestBuilder WithQueryParams(IDictionary<string, string> parameters)
    {
        foreach (var (k, v) in parameters) _queryParams[k] = v;
        return this;
    }

    public IApiRequestBuilder WithBearerToken(string token)
    {
        _headers["Authorization"] = $"Bearer {token}";
        return this;
    }

    public IApiRequestBuilder WithBasicAuth(string username, string password)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _headers["Authorization"] = $"Basic {encoded}";
        return this;
    }

    public IApiRequestBuilder WithBody(object body)
    {
        _body = body;
        return this;
    }

    public IApiRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public Task<IApiResponse<T>> GetAsync<T>(CancellationToken ct = default) =>
        _client.GetAsync<T>(_path, _queryParams, _headers, ct);

    public Task<IApiResponse<T>> PostAsync<T>(CancellationToken ct = default) =>
        _client.PostAsync<T>(_path, _body, _headers, ct);

    public Task<IApiResponse<T>> PutAsync<T>(CancellationToken ct = default) =>
        _client.PutAsync<T>(_path, _body, _headers, ct);

    public Task<IApiResponse<T>> PatchAsync<T>(CancellationToken ct = default) =>
        _client.PatchAsync<T>(_path, _body, _headers, ct);

    public Task<IApiResponse> DeleteAsync(CancellationToken ct = default) =>
        _client.DeleteAsync(_path, _headers, ct);
}
