// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Magicalizer.IntegrationTests.Diagnostics;

/// <summary>
/// The events Magicalizer logs on the start: the served DTOs and the problems of the configuration of an app.
/// </summary>
public class DiagnosticsTests : IntegrationTest
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public DiagnosticsTests(MagicalizerFactory factory) : base(factory) { }

  [Fact]
  public void Start_MagicalizedDtos_AreDescribed()
  {
    this.AssertLogged(LogLevel.Information, "The magicalized DTO ProductDto is served by MagicalizedController<ProductDto> at /api/products: GET, POST, PUT, PATCH, DELETE.");
    this.AssertLogged(LogLevel.Information, "The magicalized DTO TagDto is served by MagicalizedController<TagDto> at /api/tags: GET, POST.");
    this.AssertLogged(LogLevel.Information, "The magicalized DTO NoteDto is served by MagicalizedController<NoteDto> at /api/notes: GET (authenticated), POST (policy Notes.Edit), PUT (policy Notes.Edit), PATCH (policy Notes.Edit), DELETE (policy Notes.Edit, policy Notes.Delete).");
  }

  [Fact]
  public void Start_CustomControllerAndService_AreDescribed()
  {
    this.AssertLogged(LogLevel.Information, "The magicalized DTO PhotoDto is served by PhotosController at /api/photos: GET, POST, PUT, PATCH, DELETE (authenticated).");
    this.AssertLogged(LogLevel.Debug, "The model ProductModel is served by the custom service ProductService.");
    this.AssertLogged(LogLevel.Debug, "The model CategoryModel is served by the default service.");
  }

  [Fact]
  public void Start_DtoWithoutModel_IsReported()
  {
    this.AssertLogged(LogLevel.Warning, "The magicalized DTO BrokenDto is skipped: it doesn't implement IDto<TModel>.");
  }

  [Fact]
  public void Start_ModelWithoutEntityAndFilter_IsReported()
  {
    this.AssertLogged(LogLevel.Debug, "The model UnservedModel doesn't have a service: it doesn't implement IModel<TEntity, TFilter>.");
  }

  [Fact]
  public void Start_EntityWithoutKey_IsReported()
  {
    this.AssertLogged(LogLevel.Warning, "The model KeylessModel doesn't have a service: its entity KeylessEntity doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2> or IEntity<TKey1, TKey2, TKey3>.");
    this.AssertLogged(LogLevel.Warning, "The magicalized DTO KeylessDto is skipped: its entity KeylessEntity doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2> or IEntity<TKey1, TKey2, TKey3>.");
  }

  [Fact]
  public void Start_DtoWithoutConstructorOfModel_IsReported()
  {
    this.AssertLogged(LogLevel.Warning, "The magicalized DTO ConstructorlessDto doesn't have a constructor that takes its model NoteModel, so the requests to its controller will fail.");
  }

  [Fact]
  public void Start_UnregisteredPolicy_IsReported()
  {
    this.AssertLogged(LogLevel.Warning, "The magicalized DTO SecretDto requires the policy Missing.Policy for GET, but it isn't registered, so the requests will fail.");
  }

  private void AssertLogged(LogLevel level, string message)
  {
    // The events are logged when the host starts (the client starts it).
    IReadOnlyList<LogEntry> entries = this.Factory.Logs.Entries;

    Assert.Contains(entries, e => e.Category == "Magicalizer" && e.Level == level && e.Message == message);
  }
}
