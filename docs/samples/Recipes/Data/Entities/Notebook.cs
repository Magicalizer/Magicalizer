using Magicalizer.Data.Entities.Abstractions;

namespace App.Data.Entities;

public class Notebook : IEntity<int>
{
  public int Id { get; set; }
  public string UserId { get; set; } = null!;
  public string? Name { get; set; }
  public virtual ICollection<Note>? Notes { get; set; }
}
