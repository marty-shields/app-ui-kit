using Microsoft.Extensions.Logging;

namespace Shields.UI.Tests;

/// <summary>Keeps what a component logs: event id, level, message and the last exception.</summary>
internal sealed class RecordingLogger<T> : ILogger<T>
{
    private readonly List<(int Id, LogLevel Level, string Message)> records = [];

    public IReadOnlyList<(int Id, LogLevel Level, string Message)> Records => records;

    public Exception? LastException { get; private set; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        records.Add((eventId.Id, logLevel, formatter(state, exception)));
        LastException = exception;
    }
}
