using Magicalizer.Filters.Abstractions;

namespace App.Filters;

public class PhotoFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public ProductFilter? Product { get; set; }
  public StringFilter? Filename { get; set; }
}
