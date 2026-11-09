using System.Net;
using System.Net.Http.Json;
using App.Api.Dto;

namespace App.Tests;

public class NotebookTests : IClassFixture<AppFactory>
{
  private readonly AppFactory factory;

  public NotebookTests(AppFactory factory)
  {
    this.factory = factory;
  }

  [Fact]
  public async Task Anonymous_user_gets_401()
  {
    HttpClient client = this.factory.CreateClient();

    HttpResponseMessage response = await client.GetAsync("/v1/notebooks", TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task User_sees_only_own_notebooks()
  {
    CancellationToken cancellationToken = TestContext.Current.CancellationToken;
    HttpClient alice = this.factory.CreateClient("alice");
    HttpClient bob = this.factory.CreateClient("bob");

    HttpResponseMessage created = await alice.PostAsJsonAsync("/v1/notebooks", new Notebook { Name = "Alice's" }, cancellationToken);
    Notebook? notebook = await created.Content.ReadFromJsonAsync<Notebook>(cancellationToken);

    Assert.Equal(HttpStatusCode.Created, created.StatusCode);

    Notebook[]? bobsNotebooks = await bob.GetFromJsonAsync<Notebook[]>("/v1/notebooks", cancellationToken);
    HttpResponseMessage bobsResponse = await bob.GetAsync($"/v1/notebooks/{notebook!.Id}", cancellationToken);

    Assert.DoesNotContain(bobsNotebooks!, n => n.Id == notebook.Id);
    Assert.Equal(HttpStatusCode.NotFound, bobsResponse.StatusCode);
  }

  [Fact]
  public async Task Deleted_note_is_hidden()
  {
    CancellationToken cancellationToken = TestContext.Current.CancellationToken;
    HttpClient alice = this.factory.CreateClient("alice");

    HttpResponseMessage createdNotebook = await alice.PostAsJsonAsync("/v1/notebooks", new Notebook { Name = "Drafts" }, cancellationToken);
    Notebook? notebook = await createdNotebook.Content.ReadFromJsonAsync<Notebook>(cancellationToken);
    HttpResponseMessage createdNote = await alice.PostAsJsonAsync("/v1/notes", new Note { Notebook = notebook, Title = "Draft" }, cancellationToken);
    Note? note = await createdNote.Content.ReadFromJsonAsync<Note>(cancellationToken);

    HttpResponseMessage deleted = await alice.DeleteAsync($"/v1/notes/{note!.Id}", cancellationToken);
    Notebook? notebookWithNotes = await alice.GetFromJsonAsync<Notebook>($"/v1/notebooks/{notebook!.Id}?include=notes", cancellationToken);

    Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    Assert.Empty(notebookWithNotes!.Notes!);
  }
}
