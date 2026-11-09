// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Controllers;
using Magicalizer.Domain;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.TestHost.Photos;

/// <summary>
/// Custom controller of photos: Magicalizer doesn't create its own one for <see cref="PhotoDto"/>. The photos are
/// sorted by their filenames by default, the filenames are saved in lower case, and the number of photos is available
/// separately. It doesn't have its own route, so it gets
/// the one of the DTO.
/// </summary>
public class PhotosController : MagicalizedController<int, PhotoModel, PhotoDto, PhotoFilter>
{
  /// <summary>
  /// Creates the controller.
  /// </summary>
  /// <param name="service">Service of the photos.</param>
  public PhotosController(IService<int, PhotoModel, PhotoFilter> service) : base(service) { }

  /// <inheritdoc/>
  /// <remarks>
  /// The photos are sorted by their filenames if the request doesn't sort them.
  /// </remarks>
  [HttpGet]
  public override Task<ActionResult<IEnumerable<PhotoDto>>> GetAsync([FromQuery] PhotoFilter? filter = null, [FromQuery] IEnumerable<ISorting<PhotoModel>>? sort = null, int? offset = null, int? limit = null, [FromQuery] IEnumerable<IInclusion<PhotoModel>>? include = null, CancellationToken cancellationToken = default)
  {
    return base.GetAsync(filter, sort ?? [new Sorting<PhotoModel>(true, p => p.Filename)], offset, limit, include, cancellationToken);
  }

  /// <inheritdoc/>
  /// <remarks>
  /// The filename is saved in lower case.
  /// </remarks>
  [HttpPost]
  public override Task<ActionResult<PhotoDto>> PostAsync([FromBody] PhotoDto dto, CancellationToken cancellationToken = default)
  {
    dto.Filename = dto.Filename?.ToLowerInvariant();
    return base.PostAsync(dto, cancellationToken);
  }

  /// <summary>
  /// Counts the photos.
  /// </summary>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns>Number of photos.</returns>
  [HttpGet("count")]
  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await this.service.CountAsync(cancellationToken: cancellationToken);
  }
}
