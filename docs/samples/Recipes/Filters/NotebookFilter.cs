using Magicalizer.Filters.Abstractions;

namespace App.Filters;

// There is no UserId: the prefilter decides whose notebooks are seen, not the client.
public class NotebookFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public StringFilter? Name { get; set; }
  public EnumerableFilter<NoteFilter>? Notes { get; set; }
}
