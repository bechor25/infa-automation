namespace Company.Automation.Contracts.API;

/// <summary>
/// Thin abstraction over HTTP operations used by test projects.
/// All methods attach request/response details to the Allure report automatically.
/// </summary>
public interface IApiClient
{
    Task<IApiResponse<T>> GetAsync<T>(
        string path,
        IDictionary<string, string>? queryParams = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);

    Task<IApiResponse<T>> PostAsync<T>(
        string path,
        object? body = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);

    Task<IApiResponse<T>> PutAsync<T>(
        string path,
        object? body = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);

    Task<IApiResponse<T>> PatchAsync<T>(
        string path,
        object? body = null,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);

    Task<IApiResponse> DeleteAsync(
        string path,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);

    /// <summary>Fluent builder entry point for complex requests.</summary>
    IApiRequestBuilder NewRequest(string path);
}
