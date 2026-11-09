using Magicalizer.Filters.Abstractions;

namespace App.Filters;

public class CategoryFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public StringFilter? Name { get; set; }
  public EnumerableFilter<ProductFilter>? Products { get; set; }
}
