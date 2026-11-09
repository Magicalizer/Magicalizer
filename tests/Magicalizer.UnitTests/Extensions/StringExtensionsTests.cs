// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Extensions;

namespace Magicalizer.UnitTests.Extensions;

/// <summary>
/// The conversions of the strings: the comma-separated lists of the query parameters.
/// </summary>
public class StringExtensionsTests
{
  [Theory]
  [InlineData(null, new string[0])]
  [InlineData("", new string[0])]
  [InlineData("name", new[] { "name" })]
  [InlineData("name,-created", new[] { "name", "-created" })]
  [InlineData(" name , -created ,, ", new[] { "name", "-created" })]
  public void SplitByComma_Value_ReturnsTrimmedNonEmptySegments(string? value, string[] expected)
  {
    Assert.Equal(expected, value.SplitByComma());
  }
}
