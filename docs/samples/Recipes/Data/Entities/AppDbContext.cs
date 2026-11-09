using Microsoft.EntityFrameworkCore;

namespace App.Data.Entities;

public class AppDbContext : DbContext
{
  public const string SoftDeletionFilter = "SoftDeletion";

  public DbSet<Notebook> Notebooks { get; set; }
  public DbSet<Note> Notes { get; set; }

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Notebook>(etb => {
      etb.HasKey(e => e.Id);
      etb.Property(e => e.UserId).IsRequired().HasMaxLength(64);

      // Each request of a user looks for the notebooks by the user.
      etb.HasIndex(e => e.UserId);
      etb.ToTable("Notebooks");
    });

    #region SoftDeletion
    modelBuilder.Entity<Note>(etb => {
      etb.HasKey(e => e.Id);

      // The deleted notes are hidden from all the queries, including the included notes of a notebook.
      etb.HasQueryFilter(SoftDeletionFilter, e => !e.IsDeleted);
      etb.ToTable("Notes");
    });
    #endregion

    base.OnModelCreating(modelBuilder);
  }
}
