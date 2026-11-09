using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace App.Tests;

// Starts the application in memory with its own SQLite database, which is deleted afterwards.
public class AppFactory : WebApplicationFactory<Program>
{
  private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"recipes-{Guid.NewGuid():N}.db");

  // Creates a client of the user (the sample authenticates by the X-User header).
  public HttpClient CreateClient(string user)
  {
    HttpClient client = this.CreateClient();

    client.DefaultRequestHeaders.Add("X-User", user);
    return client;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseSetting("ConnectionStrings:Default", $"Data Source={this.databasePath};Pooling=false");
  }

  public override async ValueTask DisposeAsync()
  {
    await base.DisposeAsync();
    File.Delete(this.databasePath);
  }
}
