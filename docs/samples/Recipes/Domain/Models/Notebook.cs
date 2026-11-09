using Magicalizer.Domain.Models.Abstractions;

namespace App.Domain.Models;

public class Notebook : IModel<Data.Entities.Notebook, Filters.NotebookFilter>
{
  public int Id { get; set; }
  public string? UserId { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Note>? Notes { get; set; }

  public Notebook() { }

  public Notebook(Data.Entities.Notebook notebookEntity) : this(notebookEntity, ignoreNotes: false) { }

  public Notebook(Data.Entities.Notebook notebookEntity, bool ignoreNotes)
  {
    this.Id = notebookEntity.Id;
    this.UserId = notebookEntity.UserId;
    this.Name = notebookEntity.Name;

    if (!ignoreNotes)
      this.Notes = notebookEntity.Notes?.Select(n => new Note(n, ignoreNotebook: true)).ToList();
  }

  public Data.Entities.Notebook ToEntity()
  {
    return new Data.Entities.Notebook {
      Id = this.Id,
      UserId = this.UserId!,
      Name = this.Name
    };
  }
}
