using Microsoft.EntityFrameworkCore;

namespace App.Data.Entities;

public class AppDbContext : DbContext
{
  public DbSet<Category> Categories { get; set; }
  public DbSet<Product> Products { get; set; }
  public DbSet<Photo> Photos { get; set; }

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Category>(etb => {
      etb.HasKey(e => e.Id);
      etb.ToTable("Categories");
    });

    modelBuilder.Entity<Product>(etb => {
      etb.HasKey(e => e.Id);
      etb.ToTable("Products");
    });

    modelBuilder.Entity<Photo>(etb => {
      etb.HasKey(e => e.Id);
      etb.ToTable("Photos");
    });

    base.OnModelCreating(modelBuilder);
  }
}
