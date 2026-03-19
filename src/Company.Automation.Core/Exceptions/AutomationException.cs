namespace Company.Automation.Core.Exceptions;

/// <summary>
/// Base exception for all automation framework errors.
/// Distinguishes infrastructure failures from test-assertion failures.
/// </summary>
public class AutomationException : Exception
{
    public AutomationException(string message) : base(message) { }

    public AutomationException(string message, Exception innerException)
        : base(message, innerException) { }
}
