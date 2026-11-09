using Magicalizer.Api.Dto.Abstractions;

namespace App.Api.Dto;

[Magicalized("v1/notes")]
[AuthenticatedOnly]
public class Note : IDto<Domain.Models.Note>
{
  public int Id { get; set; }
  public Notebook? Notebook { get; set; }
  public string? Title { get; set; }
  public string? Text { get; set; }

  public Note() { }

  public Note(Domain.Models.Note noteModel) : this(noteModel, ignoreNotebook: false) { }

  public Note(Domain.Models.Note noteModel, bool ignoreNotebook)
  {
    this.Id = noteModel.Id;

    if (!ignoreNotebook)
      this.Notebook = noteModel.Notebook == null ? null : new Notebook(noteModel.Notebook, ignoreNotes: true);

    this.Title = noteModel.Title;
    this.Text = noteModel.Text;
  }

  public Domain.Models.Note ToModel()
  {
    return new Domain.Models.Note {
      Id = this.Id,
      Notebook = this.Notebook?.ToModel(),
      Title = this.Title,
      Text = this.Text
    };
  }
}
