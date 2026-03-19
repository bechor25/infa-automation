using System.Text;

namespace Company.Automation.Core.Exceptions;

/// <summary>
/// Thrown when an element cannot be found or does not reach the expected state
/// within the configured timeout.
/// </summary>
public sealed class ElementNotFoundException : AutomationException
{
    public string Selector      { get; }
    public string ExpectedState { get; }

    public ElementNotFoundException(string selector, string expectedState, Exception? innerException = null)
        : base(BuildMessage(selector, expectedState), innerException!)
    {
        Selector      = selector;
        ExpectedState = expectedState;
    }

    private static string BuildMessage(string selector, string expectedState)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Element not found or did not reach expected state.");
        sb.AppendLine($"  Selector : {selector}");
        sb.Append    ($"  State    : {expectedState}");
        return sb.ToString();
    }
}
