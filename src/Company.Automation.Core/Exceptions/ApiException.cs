using System.Net;
using System.Text;

namespace Company.Automation.Core.Exceptions;

/// <summary>
/// Thrown when an API response indicates failure (non-2xx) and the caller
/// invokes <c>EnsureSuccessStatusCode()</c>.
/// The message is formatted for immediate readability in test output.
/// The full response body is available on <see cref="ResponseBody"/>.
/// </summary>
public sealed class ApiException : AutomationException
{
    private const int BodyPreviewLength = 300;

    public HttpStatusCode StatusCode   { get; }
    public string?        ResponseBody { get; }
    public Uri?           RequestUri   { get; }
    public string?        HttpMethod   { get; }

    public ApiException(
        HttpStatusCode statusCode,
        string?        responseBody,
        Uri?           requestUri,
        string?        httpMethod = null)
        : base(BuildMessage(statusCode, responseBody, requestUri, httpMethod))
    {
        StatusCode   = statusCode;
        ResponseBody = responseBody;
        RequestUri   = requestUri;
        HttpMethod   = httpMethod;
    }

    private static string BuildMessage(
        HttpStatusCode statusCode,
        string?        body,
        Uri?           uri,
        string?        method)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"API request failed: {method ?? "HTTP"} {uri?.ToString() ?? "(unknown URL)"}");
        sb.AppendLine($"  Status : {(int)statusCode} ({statusCode})");

        if (string.IsNullOrWhiteSpace(body))
        {
            sb.Append("  Body   : (empty)");
        }
        else
        {
            var preview = body.Length > BodyPreviewLength
                ? body[..BodyPreviewLength].Trim() + $"… (truncated, {body.Length:N0} bytes total)"
                : body.Trim();
            sb.Append($"  Body   : {preview}");
        }

        return sb.ToString();
    }
}
