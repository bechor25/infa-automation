using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Company.Automation.Core.Logging;

/// <summary>
/// Configures a Serilog logger wired into Microsoft.Extensions.Logging.
/// Call once at test host startup.
/// </summary>
public static class AutomationLoggerFactory
{
    private static bool _initialized;
    private static readonly object _lock = new();

    public static ILoggerFactory Create(string logDirectory = "logs", LogEventLevel minimumLevel = LogEventLevel.Debug)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                Directory.CreateDirectory(logDirectory);

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(minimumLevel)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: Path.Combine(logDirectory, "automation-.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate:
                        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                _initialized = true;
            }
        }

        return new Serilog.Extensions.Logging.SerilogLoggerFactory(Log.Logger, dispose: false);
    }

    public static void CloseAndFlush() => Log.CloseAndFlush();
}
