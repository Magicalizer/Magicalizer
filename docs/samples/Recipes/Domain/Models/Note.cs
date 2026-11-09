using Magicalizer.Domain.Models.Abstractions;

namespace App.Domain.Models;

public class Note : IModel<Data.Entities.Note, Filters.NoteFilter>
{
  public int Id { get; set; }
  public Notebook? Notebook { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }

  public Note() { }

  public Note(Data.Entities.Note noteEntity) : this(noteEntity, ignoreNotebook: false) { }

  public Note(Data.Entities.Note noteEntity, bool ignoreNotebook)
  {
    this.Id = noteEntity.Id;

    if (!ignoreNotebook)
      this.Notebook = noteEntity.Notebook == null ? new Notebook { Id = noteEntity.NotebookId } : new Notebook(noteEntity.Notebook, ignoreNotes: true);

    this.Title = noteEntity.Title;
    this.Text = noteEntity.Text;
  }

  public Data.Entities.Note ToEntity()
  {
    return new Data.Entities.Note {
      Id = this.Id,
      NotebookId = this.Notebook?.Id ?? 0,
      Title = this.Title,
      Text = this.Text
    };
  }
}
