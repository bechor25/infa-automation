using System.Net;

namespace Company.Automation.Contracts.API;

/// <summary>Non-generic response envelope — status, headers, raw body.</summary>
public interface IApiResponse
{
    HttpStatusCode StatusCode { get; }
    bool IsSuccessful { get; }
    string RawBody { get; }
    IDictionary<string, string> Headers { get; }
    Uri? RequestUri { get; }
    string HttpMethod { get; }

    /// <summary>Throws <see cref="Company.Automation.Core.Exceptions.ApiException"/> if status is not 2xx.</summary>
    void EnsureSuccessStatusCode();
}

/// <summary>Generic response envelope with deserialized body.</summary>
public interface IApiResponse<out T> : IApiResponse
{
    T? Data { get; }
}
