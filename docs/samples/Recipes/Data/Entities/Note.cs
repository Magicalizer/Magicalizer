using Magicalizer.Data.Entities.Abstractions;

namespace App.Data.Entities;

public class Note : IEntity<int>
{
  public int Id { get; set; }
  public int NotebookId { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }
  public bool IsDeleted { get; set; }
  public virtual Notebook? Notebook { get; set; }
}
