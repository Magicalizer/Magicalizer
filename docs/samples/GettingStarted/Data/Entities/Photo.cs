using Magicalizer.Data.Entities.Abstractions;

namespace App.Data.Entities;

public class Photo : IEntity<int>
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public string? Filename { get; set; }
  public virtual Product? Product { get; set; }
}
