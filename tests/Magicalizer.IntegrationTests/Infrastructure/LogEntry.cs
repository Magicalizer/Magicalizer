// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;

namespace Magicalizer.IntegrationTests.Infrastructure;

/// <summary>
/// Recorded log message.
/// </summary>
public record LogEntry(string Category, LogLevel Level, string Message);
