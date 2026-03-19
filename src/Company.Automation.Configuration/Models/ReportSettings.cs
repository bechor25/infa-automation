namespace Company.Automation.Configuration.Models;

public sealed class ReportSettings
{
    /// <summary>Directory where Allure results JSON files are written.</summary>
    public string AllureResultsDirectory { get; init; } = "allure-results";

    /// <summary>Include environment properties file in Allure results.</summary>
    public bool WriteEnvironmentProperties { get; init; } = true;

    /// <summary>Attach Serilog log file to the report on failure.</summary>
    public bool AttachLogOnFailure { get; init; } = true;
}
