// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Magicalizer.IntegrationTests.Infrastructure;

/// <summary>
/// Logger provider that records the messages of the test host (e.g. to check the diagnostics of Magicalizer).
/// </summary>
public sealed class LogRecorder : ILoggerProvider
{
  private readonly ConcurrentQueue<LogEntry> entries = new ConcurrentQueue<LogEntry>();

  /// <summary>
  /// Recorded messages in the order they were logged.
  /// </summary>
  public IReadOnlyList<LogEntry> Entries => this.entries.ToList();

  /// <inheritdoc/>
  public ILogger CreateLogger(string categoryName)
  {
    return new Logger(categoryName, this.entries);
  }

  /// <inheritdoc/>
  public void Dispose()
  {
  }

  private sealed class Logger : ILogger
  {
    private readonly string category;
    private readonly ConcurrentQueue<LogEntry> entries;

    public Logger(string category, ConcurrentQueue<LogEntry> entries)
    {
      this.category = category;
      this.entries = entries;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      this.entries.Enqueue(new LogEntry(this.category, logLevel, formatter(state, exception)));
    }
  }
}
