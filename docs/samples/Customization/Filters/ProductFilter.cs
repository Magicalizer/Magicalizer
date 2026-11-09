using Magicalizer.Filters.Abstractions;

namespace App.Filters;

public class ProductFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public CategoryFilter? Category { get; set; }
  public StringFilter? Name { get; set; }
  public DecimalFilter? Price { get; set; }
  public DateTimeFilter? Created { get; set; }
  public EnumerableFilter<PhotoFilter>? Photos { get; set; }
  public BooleanFilter? IsArchived { get; set; }

  // A search text the service turns into the conditions (see ProductService).
  public string? Search { get; set; }
}
