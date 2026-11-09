using Magicalizer.Api.Dto.Abstractions;

namespace App.Api.Dto;

[Magicalized("v1/notebooks")]
[AuthenticatedOnly]
public class Notebook : IDto<Domain.Models.Notebook>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Note>? Notes { get; set; }

  public Notebook() { }

  public Notebook(Domain.Models.Notebook notebookModel) : this(notebookModel, ignoreNotes: false) { }

  public Notebook(Domain.Models.Notebook notebookModel, bool ignoreNotes)
  {
    this.Id = notebookModel.Id;
    this.Name = notebookModel.Name;

    if (!ignoreNotes)
      this.Notes = notebookModel.Notes?.Select(n => new Note(n, ignoreNotebook: true)).ToList();
  }

  public Domain.Models.Notebook ToModel()
  {
    return new Domain.Models.Notebook {
      Id = this.Id,
      Name = this.Name
    };
  }
}
