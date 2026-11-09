namespace App.Domain;

// The user the code works for. The domain doesn't depend on HTTP, so it gets the user through this interface.
public interface ICurrentUser
{
  string? Id { get; }
}
