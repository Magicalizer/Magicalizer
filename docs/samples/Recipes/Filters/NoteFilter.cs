using Magicalizer.Filters.Abstractions;

namespace App.Filters;

public class NoteFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public NotebookFilter? Notebook { get; set; }
  public StringFilter? Title { get; set; }
  public StringFilter? Text { get; set; }
}
