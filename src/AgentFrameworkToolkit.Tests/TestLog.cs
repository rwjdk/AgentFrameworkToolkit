using Microsoft.Extensions.Logging;

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

namespace AgentFrameworkToolkit.Tests;

public sealed class TestLogger : ILogger
{
    public List<string> Messages = [];

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        Messages.Add(formatter(state, exception));
    }
}

public sealed class TestLoggerFactory : ILoggerFactory
{
    public TestLogger Logger = new();

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return Logger;
    }

    public void Dispose()
    {
    }
}
