using System.Text;

namespace Company.Automation.Core.Exceptions;

/// <summary>
/// Thrown when a UI action fails after all retry attempts are exhausted.
/// Formats a human-readable message that surfaces the root cause immediately,
/// without requiring the reader to inspect InnerException manually.
/// </summary>
public sealed class UIActionException : AutomationException
{
    public string ActionDescription { get; }

    public UIActionException(string actionDescription, Exception innerException)
        : base(BuildMessage(actionDescription, innerException), innerException)
    {
        ActionDescription = actionDescription;
    }

    private static string BuildMessage(string action, Exception inner)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"UI action failed: \"{action}\"");
        sb.AppendLine($"  Error : {FirstLine(inner.Message)}");
        sb.Append    ($"  Type  : {inner.GetType().Name}");
        return sb.ToString();
    }

    /// <summary>Returns the first non-empty line of a (possibly multi-line) message.</summary>
    private static string FirstLine(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "(no message)";
        foreach (var line in message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0) return trimmed;
        }
        return message.Trim();
    }
}
