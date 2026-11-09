// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Extensions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// The paging applied to a query in memory (see <see cref="Items"/>).
/// </summary>
public class PagingTests
{
  [Theory]
  [InlineData(null, null, "1,2,3")]
  [InlineData(1, null, "2,3")]
  [InlineData(null, 2, "1,2")]
  [InlineData(1, 1, "2")]
  [InlineData(5, 1, "")]
  public void ApplyPaging_OffsetAndLimit_ReturnPage(int? offset, int? limit, string expected)
  {
    Assert.Equal(expected, Items.Ids(Items.Create().ApplyPaging(offset, limit)));
  }
}
