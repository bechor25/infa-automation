namespace Company.Automation.Contracts.API;

/// <summary>Fluent builder for composing complex API requests.</summary>
public interface IApiRequestBuilder
{
    IApiRequestBuilder WithHeader(string name, string value);
    IApiRequestBuilder WithHeaders(IDictionary<string, string> headers);
    IApiRequestBuilder WithQueryParam(string name, string value);
    IApiRequestBuilder WithQueryParams(IDictionary<string, string> parameters);
    IApiRequestBuilder WithBearerToken(string token);
    IApiRequestBuilder WithBasicAuth(string username, string password);
    IApiRequestBuilder WithBody(object body);
    IApiRequestBuilder WithTimeout(TimeSpan timeout);

    Task<IApiResponse<T>> GetAsync<T>(CancellationToken ct = default);
    Task<IApiResponse<T>> PostAsync<T>(CancellationToken ct = default);
    Task<IApiResponse<T>> PutAsync<T>(CancellationToken ct = default);
    Task<IApiResponse<T>> PatchAsync<T>(CancellationToken ct = default);
    Task<IApiResponse> DeleteAsync(CancellationToken ct = default);
}
