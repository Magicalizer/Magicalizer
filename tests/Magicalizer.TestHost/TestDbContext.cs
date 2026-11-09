// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.TestHost.Categories;
using Magicalizer.TestHost.Notes;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Products;
using Magicalizer.TestHost.Stocks;
using Magicalizer.TestHost.Tags;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.TestHost;

/// <summary>
/// Database context of the test host.
/// </summary>
public class TestDbContext : DbContext
{
  /// <summary>
  /// Creates the context.
  /// </summary>
  public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

  /// <inheritdoc/>
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<CategoryEntity>(etb => {
      etb.HasKey(e => e.Id);
      etb.Property(e => e.Id).ValueGeneratedOnAdd();
      etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
      etb.ToTable("Categories");
    });

    modelBuilder.Entity<ProductEntity>(etb => {
      etb.HasKey(e => e.Id);
      etb.Property(e => e.Id).ValueGeneratedOnAdd();
      etb.Property(e => e.Name).IsRequired().HasMaxLength(64);

      // The diet is stored by its name, so the filters are checked with a value converter too.
      etb.Property(e => e.Diet).HasConversion<string>().HasMaxLength(16);

      // SQLite can't compare the times with offsets, so they are stored as UTC ticks there.
      if (this.Database.IsSqlite())
        etb.Property(e => e.Updated).HasConversion(v => v!.Value.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));

      // A category with products can't be deleted (the database restricts it).
      etb.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
      etb.ToTable("Products");
    });

    modelBuilder.Entity<PhotoEntity>(etb => {
      etb.HasKey(e => e.Id);
      etb.Property(e => e.Id).ValueGeneratedOnAdd();
      etb.Property(e => e.Filename).IsRequired().HasMaxLength(64);
      etb.HasOne(e => e.Product).WithMany(p => p.Photos).HasForeignKey(e => e.ProductId);
      etb.ToTable("Photos");
    });

    modelBuilder.Entity<TagEntity>(etb => {
      etb.HasKey(e => e.Id);
      etb.Property(e => e.Id).ValueGeneratedOnAdd();
      etb.Property(e => e.Name).IsRequired().HasMaxLength(64);
      etb.ToTable("Tags");
    });

    modelBuilder.Entity<ProductTagEntity>(etb => {
      etb.HasKey(e => new { e.ProductId, e.TagId });
      etb.HasOne(e => e.Product).WithMany(p => p.Tags).HasForeignKey(e => e.ProductId);
      etb.HasOne(e => e.Tag).WithMany().HasForeignKey(e => e.TagId);
      etb.ToTable("ProductTags");
    });

    modelBuilder.Entity<StockEntity>(etb => {
      etb.HasKey(e => new { e.WarehouseCode, e.ProductId, e.BatchNumber });
      etb.Property(e => e.WarehouseCode).HasMaxLength(16);
      etb.ToTable("Stocks");
    });

    modelBuilder.Entity<NoteEntity>(etb => {
      etb.HasKey(e => e.Id);
      etb.Property(e => e.Id).ValueGeneratedOnAdd();
      etb.Property(e => e.Text).IsRequired();
      etb.ToTable("Notes");
    });

    base.OnModelCreating(modelBuilder);
  }
}
