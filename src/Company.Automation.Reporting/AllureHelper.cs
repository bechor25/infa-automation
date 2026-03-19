using System.Text;
using Allure.Net.Commons;

namespace Company.Automation.Reporting;

/// <summary>
/// Centralised helper for all Allure reporting operations used by the UI and API layers.
/// All methods are static so they can be called from anywhere without injection overhead.
/// </summary>
public static class AllureHelper
{
    // ── Steps ─────────────────────────────────────────────────────────────────

    /// <summary>Wraps a synchronous action in a named Allure step.</summary>
    public static void Step(string name, Action action) =>
        AllureApi.Step(name, action);

    /// <summary>Wraps an asynchronous action in a named Allure step.</summary>
    public static async Task StepAsync(string name, Func<Task> action) =>
        await AllureApi.Step(name, action);

    /// <summary>Wraps an async function that returns a value in a named Allure step.</summary>
    public static async Task<T> StepAsync<T>(string name, Func<Task<T>> action)
    {
        T result = default!;
        await AllureApi.Step(name, async () => { result = await action(); });
        return result;
    }

    // ── Attachments ───────────────────────────────────────────────────────────

    /// <summary>Attaches a screenshot (PNG bytes) to the current Allure test/step.</summary>
    public static void AttachScreenshot(string name, byte[] pngBytes)
    {
        if (pngBytes is { Length: > 0 })
            AllureApi.AddAttachment(name, "image/png", pngBytes, "png");
    }

    /// <summary>Attaches a plain-text string (e.g. JSON request/response).</summary>
    public static void AttachText(string name, string content, string mimeType = "text/plain")
    {
        if (!string.IsNullOrEmpty(content))
            AllureApi.AddAttachment(name, mimeType, Encoding.UTF8.GetBytes(content), "txt");
    }

    /// <summary>Attaches JSON content with the application/json MIME type.</summary>
    public static void AttachJson(string name, string json) =>
        AttachText(name, json, "application/json");

    /// <summary>Attaches binary content (e.g. video, trace zip).</summary>
    public static void AttachFile(string name, string mimeType, byte[] content, string extension) =>
        AllureApi.AddAttachment(name, mimeType, content, extension);

    /// <summary>Attaches a file from disk.</summary>
    public static void AttachFilePath(string name, string mimeType, string filePath)
    {
        if (File.Exists(filePath))
        {
            var bytes = File.ReadAllBytes(filePath);
            var ext = Path.GetExtension(filePath).TrimStart('.');
            AllureApi.AddAttachment(name, mimeType, bytes, ext);
        }
    }

    // ── API evidence convenience methods ─────────────────────────────────────

    public static void AttachApiRequest(string method, string url, string? body, IDictionary<string, string>? headers)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{method} {url}");
        if (headers is { Count: > 0 })
        {
            sb.AppendLine("Headers:");
            foreach (var (k, v) in headers)
                sb.AppendLine($"  {k}: {v}");
        }
        if (!string.IsNullOrEmpty(body))
        {
            sb.AppendLine("Body:");
            sb.AppendLine(body);
        }
        AttachText("Request", sb.ToString());
    }

    public static void AttachApiResponse(int statusCode, string? body, IDictionary<string, string>? headers)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Status: {statusCode}");
        if (headers is { Count: > 0 })
        {
            sb.AppendLine("Headers:");
            foreach (var (k, v) in headers)
                sb.AppendLine($"  {k}: {v}");
        }
        if (!string.IsNullOrEmpty(body))
        {
            sb.AppendLine("Body:");
            sb.AppendLine(body);
        }
        AttachText("Response", sb.ToString());
    }

    // ── Labels / metadata ────────────────────────────────────────────────────

    public static void AddLabel(string name, string value) =>
        AllureApi.AddLabel(name, value);

    public static void SetSeverity(SeverityLevel level) =>
        AllureApi.SetSeverity(level);

    public static void AddLink(string name, string url, string? type = null) =>
        AllureApi.AddLink(name, url, type ?? "link");

    // ── Environment properties ────────────────────────────────────────────────

    /// <summary>
    /// Writes an environment.properties file into the Allure results directory
    /// so the report shows environment metadata.
    /// </summary>
    public static void WriteEnvironmentProperties(
        string resultsDirectory,
        IDictionary<string, string> properties)
    {
        try
        {
            Directory.CreateDirectory(resultsDirectory);
            var lines = properties.Select(kv => $"{kv.Key}={kv.Value}");
            File.WriteAllLines(Path.Combine(resultsDirectory, "environment.properties"), lines);
        }
        catch
        {
            // Non-fatal: environment metadata missing from report is acceptable
        }
    }

}
