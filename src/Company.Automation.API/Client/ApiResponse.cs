using System.Net;
using Company.Automation.Contracts.API;
using Company.Automation.Core.Exceptions;

namespace Company.Automation.API.Client;

/// <summary>Non-generic API response envelope.</summary>
internal sealed class ApiResponse : IApiResponse
{
    public HttpStatusCode StatusCode { get; init; }
    public bool IsSuccessful => (int)StatusCode is >= 200 and <= 299;
    public string RawBody { get; init; } = string.Empty;
    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    public Uri? RequestUri { get; init; }
    public string HttpMethod { get; init; } = string.Empty;

    public void EnsureSuccessStatusCode()
    {
        if (!IsSuccessful)
            throw new ApiException(StatusCode, RawBody, RequestUri, HttpMethod);
    }
}

/// <summary>Generic API response envelope with a deserialized data payload.</summary>
internal sealed class ApiResponse<T> : IApiResponse<T>
{
    public HttpStatusCode StatusCode { get; init; }
    public bool IsSuccessful => (int)StatusCode is >= 200 and <= 299;
    public string RawBody { get; init; } = string.Empty;
    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    public Uri? RequestUri { get; init; }
    public string HttpMethod { get; init; } = string.Empty;
    public T? Data { get; init; }

    public void EnsureSuccessStatusCode()
    {
        if (!IsSuccessful)
            throw new ApiException(StatusCode, RawBody, RequestUri, HttpMethod);
    }
}
