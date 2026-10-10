// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Magicalizer.Diagnostics;

/// <summary>
/// Collects the events that happen while Magicalizer is being configured (when the logging is not available yet),
/// so they can be logged when the application starts.
/// </summary>
internal sealed class MagicalizerDiagnostics
{
  private readonly List<Entry> entries = [];
  private readonly HashSet<string> keys = [];
  private readonly object @lock = new();

  /// <summary>
  /// The collected events in the order they happened.
  /// </summary>
  public IReadOnlyList<Entry> Entries
  {
    get
    {
      lock (this.@lock)
        return this.entries.ToList();
    }
  }

  /// <summary>
  /// Adds an event.
  /// </summary>
  /// <param name="level">The log level of the event.</param>
  /// <param name="message">The message template of the event.</param>
  /// <param name="args">The arguments of the message template.</param>
  public void Add(LogLevel level, string message, params object?[] args)
  {
    // The same events may be reported several times (e.g. if the controllers are discovered more than once), so they are deduplicated.
    string key = $"{level}|{message}|{string.Join("|", args)}";

    lock (this.@lock)
      if (this.keys.Add(key))
        this.entries.Add(new Entry(level, message, args));
  }

  /// <summary>
  /// Represents a collected event.
  /// </summary>
  /// <param name="Level">The log level of the event.</param>
  /// <param name="Message">The message template of the event.</param>
  /// <param name="Args">The arguments of the message template.</param>
  public record Entry(LogLevel Level, string Message, object?[] Args);
}
