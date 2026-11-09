// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.IntegrationTests.Infrastructure;
using Magicalizer.TestHost.Categories;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Products;
using Magicalizer.TestHost.Tags;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// Host of the catalog tests: creates the data the tests query once, before the tests of the class start.
/// </summary>
public class CatalogTestsFactory : MagicalizerFactory, IAsyncLifetime
{
  /// <summary>
  /// The created data.
  /// </summary>
  public CatalogDataSet DataSet { get; private set; } = null!;

  /// <inheritdoc/>
  public async ValueTask InitializeAsync()
  {
    // The products (1-6) of the data set, the 6th one is archived (hidden from the users without the permission):
    //  #  name              category  price  qty  available  created     description  tags
    //  1  Margherita        Pizza     100    10   true       2026-01-10  Classic      Hot (0)
    //  2  Pepperoni         Pizza     150    0    false      2026-02-15  -            Hot (0), New (1)
    //  3  Four Cheese       Pizza     200.5  5    true       2026-03-20  Cheesy       -
    //  4  Borscht           Soups     90     7    true       2025-12-31  -            New (0)
    //  5  Minestrone        Soups     90     0    false      2026-01-05  Vegetable    -
    //  6  Yesterday's soup  Soups     10     1    true       2024-01-01  -            -
    // Their diets and update times (in UTC here, but they are set with different offsets, so the filters compare the
    // moments, not the clock times):
    //  #  diet        updated
    //  1  Vegetarian  2026-01-12 08:00
    //  2  Regular     -
    //  3  Vegetarian  2026-03-21 00:00
    //  4  Regular     -
    //  5  Vegan       2026-01-06 06:00
    //  6  Regular     -
    // The photos are named after their products: margherita-1.jpg, margherita-2.jpg, pepperoni-1.jpg, borscht-1.png
    // (the only PNG one) and yesterdays-soup-1.jpg. The categories are Pizza ("Italian"), Soups and Напої ("drinks" in
    // Ukrainian, without products).
    CategoryEntity pizza = new CategoryEntity { Name = "Pizza", Description = "Italian" };
    CategoryEntity soups = new CategoryEntity { Name = "Soups" };
    CategoryEntity drinks = new CategoryEntity { Name = "Напої" };
    TagEntity hot = new TagEntity { Name = "Hot" };
    TagEntity @new = new TagEntity { Name = "New" };
    ProductEntity[] products = [
      CreateProduct(pizza, "Margherita", 100m, 10, true, new DateTime(2026, 1, 10), "Classic", "margherita-1.jpg", "margherita-2.jpg"),
      CreateProduct(pizza, "Pepperoni", 150m, 0, false, new DateTime(2026, 2, 15), null, "pepperoni-1.jpg"),
      CreateProduct(pizza, "Four Cheese", 200.5m, 5, true, new DateTime(2026, 3, 20), "Cheesy"),
      CreateProduct(soups, "Borscht", 90m, 7, true, new DateTime(2025, 12, 31), null, "borscht-1.png"),
      CreateProduct(soups, "Minestrone", 90m, 0, false, new DateTime(2026, 1, 5), "Vegetable"),
      CreateProduct(soups, "Yesterday's soup", 10m, 1, true, new DateTime(2024, 1, 1), null, "yesterdays-soup-1.jpg")
    ];

    products[5].IsArchived = true;
    products[0].Diet = Diet.Vegetarian;
    products[2].Diet = Diet.Vegetarian;
    products[4].Diet = Diet.Vegan;
    products[0].Updated = new DateTimeOffset(2026, 1, 12, 10, 0, 0, TimeSpan.FromHours(2));
    products[2].Updated = new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero);
    products[4].Updated = new DateTimeOffset(2026, 1, 6, 1, 0, 0, TimeSpan.FromHours(-5));
    products[0].Tags = [new ProductTagEntity { Tag = hot, Position = 0 }];
    products[1].Tags = [new ProductTagEntity { Tag = hot, Position = 0 }, new ProductTagEntity { Tag = @new, Position = 1 }];
    products[3].Tags = [new ProductTagEntity { Tag = @new, Position = 0 }];

    await this.WithDbAsync(async db => {
      db.AddRange(drinks);
      db.AddRange(products);
      await db.SaveChangesAsync();
    });

    this.DataSet = new CatalogDataSet(
      products.Select(p => p.Id).ToArray(),
      new Dictionary<string, int> {
        ["Pizza"] = pizza.Id,
        ["Soups"] = soups.Id,
        ["Напої"] = drinks.Id,
        ["Hot"] = hot.Id,
        ["New"] = @new.Id,
        ["margherita-1"] = products[0].Photos!.First().Id,
        ["margherita-2"] = products[0].Photos!.Last().Id,
        ["pepperoni-1"] = products[1].Photos!.Single().Id,
        ["borscht-1"] = products[3].Photos!.Single().Id
      },
      products.Select((p, i) => (p, i)).ToDictionary(pi => $"code-{pi.i + 1}", pi => pi.p.Code)
    );
  }

  private static ProductEntity CreateProduct(CategoryEntity category, string name, decimal price, int quantity, bool isAvailable, DateTime created, string? description, params string[] photos)
  {
    return new ProductEntity {
      Category = category,
      Name = name,
      Price = price,
      Quantity = quantity,
      IsAvailable = isAvailable,
      Created = DateTime.SpecifyKind(created, DateTimeKind.Utc),
      Code = Guid.NewGuid(),
      Description = description,
      Photos = photos.Select(f => new PhotoEntity { Filename = f }).ToList()
    };
  }
}
