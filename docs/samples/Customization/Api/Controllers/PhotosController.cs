using App.Domain.Models;
using App.Filters;
using Magicalizer.Api.Controllers;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

public class PhotosController : MagicalizedController<int, Photo, Dto.Photo, PhotoFilter>
{
  public PhotosController(IService<int, Photo, PhotoFilter> service) : base(service) { }

  [HttpGet("count")]
  public Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return this.service.CountAsync(cancellationToken: cancellationToken);
  }
}
