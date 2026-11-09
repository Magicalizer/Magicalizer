// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;

namespace Magicalizer.Diagnostics;

/// <summary>
/// Collects events that happen while Magicalizer is being configured (when logging isn't available yet), so they can be
/// logged when the app starts. The events that happen after that (e.g. if controllers are discovered again) are logged
/// at once.
/// </summary>
internal sealed class MagicalizerDiagnostics
{
  private readonly List<Entry> entries = [];
  private readonly HashSet<string> keys = [];
  private readonly Lock @lock = new();
  private ILogger? logger;

  /// <summary>
  /// Adds an event: collects it until the logging starts, then logs it.
  /// </summary>
  /// <param name="level">Log level of the event.</param>
  /// <param name="message">Message template of the event.</param>
  /// <param name="args">Arguments of the message template.</param>
  public void Add(LogLevel level, string message, params object?[] args)
  {
    // The same events may be reported several times (e.g. if controllers are discovered more than once), so they are
    // deduplicated.
    string key = $"{level}|{message}|{string.Join("|", args)}";

    lock (this.@lock)
    {
      if (!this.keys.Add(key)) return;

      if (this.logger == null)
        this.entries.Add(new Entry(level, message, args));

      else this.logger.Log(level, message, args);
    }
  }

  /// <summary>
  /// Logs the collected events and starts logging the next ones at once. The collected events aren't kept.
  /// </summary>
  /// <param name="logger">Logger to log the events to.</param>
  public void StartLogging(ILogger logger)
  {
    lock (this.@lock)
    {
      foreach (Entry entry in this.entries)
        logger.Log(entry.Level, entry.Message, entry.Args);

      this.entries.Clear();
      this.logger = logger;
    }
  }

  /// <summary>
  /// Event collected until the logging starts.
  /// </summary>
  /// <param name="Level">Log level of the event.</param>
  /// <param name="Message">Message template of the event.</param>
  /// <param name="Args">Arguments of the message template.</param>
  private sealed record Entry(LogLevel Level, string Message, object?[] Args);
}
