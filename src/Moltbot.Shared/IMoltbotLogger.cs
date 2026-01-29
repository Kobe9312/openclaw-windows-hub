namespace Moltbot.Shared;

/// <summary>
/// Simple logger interface for the gateway client.
/// Implementations can write to file, console, debug output, etc.
/// </summary>
public interface IMoltbotLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
}

/// <summary>
/// Default no-op logger for when logging isn't needed.
/// </summary>
public class NullLogger : IMoltbotLogger
{
    public static readonly NullLogger Instance = new();
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message, Exception? ex = null) { }
}

/// <summary>
/// Console logger for simple debugging.
/// </summary>
public class ConsoleLogger : IMoltbotLogger
{
    public void Info(string message) => Console.WriteLine($"[INFO] {message}");
    public void Warn(string message) => Console.WriteLine($"[WARN] {message}");
    public void Error(string message, Exception? ex = null) => 
        Console.WriteLine($"[ERROR] {message}{(ex != null ? $": {ex.Message}" : "")}");
}

