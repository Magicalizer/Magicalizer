using Magicalizer.Data.Entities.Abstractions;

namespace App.Data.Entities;

public class Product : IEntity<int>
{
  public int Id { get; set; }
  public int CategoryId { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public virtual Category? Category { get; set; }
  public virtual ICollection<Photo>? Photos { get; set; }
}
