using Magicalizer.Data.Entities.Abstractions;

namespace App.Data.Entities;

public class Category : IEntity<int>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public virtual ICollection<Product>? Products { get; set; }
}
