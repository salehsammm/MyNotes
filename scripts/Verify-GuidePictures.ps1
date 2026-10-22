param([switch]$Serve)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$fixtureRoot = Join-Path ([IO.Path]::GetTempPath()) ("MyNotes-GuidePictures-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
$reference = [System.Security.SecurityElement]::Escape((Join-Path $projectRoot 'MyNotes.csproj'))
@"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup><TargetFramework>net9.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><ProjectReference Include="$reference" /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $fixtureRoot 'Verification.csproj')
@'
@page "/"
@using static Microsoft.AspNetCore.Components.Web.RenderMode
<!DOCTYPE html>
<html lang="en">
<head><base href="/" /><meta name="viewport" content="width=device-width, initial-scale=1" />
<link rel="stylesheet" href="/moviereview/lib/bootstrap/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="/moviereview/app.css" /><link rel="stylesheet" href="/verification-styles.css" /></head>
<body style="padding:24px">
<h1>Disposable vocabulary picture verification</h1>
<FixturePanel @rendermode="InteractiveServer" />
<script src="/_framework/blazor.web.js"></script>
</body></html>
'@ | Set-Content -LiteralPath (Join-Path $fixtureRoot 'VerificationApp.razor')
@'
@using Microsoft.EntityFrameworkCore
@inject IDbContextFactory<MovieReviews.Data.AppDbContext> Factory
<div class="vocab-panel" style="position:relative; min-height:400px">
    <h3>Vocabulary</h3>
    <div class="vocab-group">
        <MovieReviews.Components.GuidePictureGallery SectionId="@_sectionId" SectionName="Sample vocabulary group" />
        <details class="vocab-section"><summary>Sample vocabulary group</summary><p>Sample term and description</p></details>
    </div>
</div>
@code {
    private int _sectionId;
    protected override async Task OnInitializedAsync()
    {
        await using var db = await Factory.CreateDbContextAsync();
        _sectionId = await db.GuideSections.Where(s => s.Name == "Picture verification A").Select(s => s.Id).SingleAsync();
    }
}
'@ | Set-Content -LiteralPath (Join-Path $fixtureRoot 'FixturePanel.razor')
@'
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MovieReviews.Data;
using MovieReviews.Services;

var root = args[0];
var configuration = new ConfigurationBuilder().SetBasePath(root).AddJsonFile("appsettings.json").Build();
var connection = new SqlConnectionStringBuilder(configuration.GetConnectionString("MovieReview"))
{ InitialCatalog = "MyNotesGuidePicturesVerification_" + Guid.NewGuid().ToString("N") };
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection.ConnectionString).Options;
var factory = new VerificationFactory(options);
await using var database = factory.CreateDbContext();
try
{
    await database.Database.MigrateAsync();
    Check(!(await database.Database.GetPendingMigrationsAsync()).Any(), "All migrations applied to a disposable database");
    var a = new GuideSection { Name = "Picture verification A" };
    var b = new GuideSection { Name = "Picture verification B" };
    database.AddRange(a, b);
    await database.SaveChangesAsync();
    var service = new GuideService(factory);
    var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");
    await service.AddPictureAsync(a.Id, "../First.png", png);
    await service.AddPictureAsync(a.Id, "Second.png", png);
    var pictures = await service.GetPicturesAsync(a.Id);
    Check(pictures.Count == 2 && pictures[0].FileName == "First.png", "Multiple uploads persist in order and filenames are sanitized");
    var stored = await database.GuidePictures.AsNoTracking().SingleAsync(p => p.Id == pictures[0].Id);
    Check(stored.Content.SequenceEqual(png) && stored.ContentType == "image/png", "Stored image bytes and detected MIME type round-trip");
    Check((await service.GetPicturesAsync(b.Id)).Count == 0, "Pictures stay with their vocabulary group");
    await service.DeletePictureAsync(b.Id, pictures[0].Id);
    Check((await service.GetPicturesAsync(a.Id)).Count == 2, "Deleting through a different group cannot remove a picture");
    await Reject<ArgumentException>(() => service.AddPictureAsync(a.Id, "fake.png", "not a picture"u8.ToArray()), "Reject invalid image content");
    await Reject<ArgumentException>(() => service.AddPictureAsync(a.Id, "empty.png", []), "Reject empty files");
    await Reject<ArgumentException>(() => service.AddPictureAsync(a.Id, "large.png", new byte[GuideService.MaxPictureBytes + 1]), "Reject files over 10 MB");
    await Reject<InvalidOperationException>(() => service.AddPictureAsync(int.MaxValue, "missing.png", png), "Reject uploads to a missing group");
    await service.DeletePictureAsync(a.Id, pictures[0].Id);
    Check((await service.GetPicturesAsync(a.Id)).Count == 1, "Delete only the selected picture");
    await service.AddPictureAsync(b.Id, "cascade.png", png);
    await service.DeleteSectionAsync(b.Id);
    Check((await service.GetPicturesAsync(b.Id)).Count == 0, "Group deletion cascades to its pictures");
    await service.AddPictureAsync(a.Id, "Third.png", png);
    Console.WriteLine("All vocabulary picture persistence checks passed.");
    if (args.Contains("--serve"))
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [], ContentRootPath = root });
        builder.WebHost.UseUrls("http://localhost:5058").UseStaticWebAssets();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddSingleton<IDbContextFactory<AppDbContext>>(factory);
        builder.Services.AddScoped<GuideService>();
        var app = builder.Build();
        app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(root, "wwwroot")) });
        app.UseAntiforgery();
        app.MapGet("/verification-styles.css", () => Results.File(Path.Combine(root, "obj/Debug/net9.0/scopedcss/bundle/MyNotes.styles.css"), "text/css"));
        app.MapPost("/verification/stop", (IHostApplicationLifetime lifetime) =>
        {
            lifetime.StopApplication();
            return Results.Ok();
        });
        // This fixture serves only its disposable database. Production image URLs retain PrivateArea protection.
        app.MapGet("/moviereview/guide/pictures/{id:int}", async Task<IResult> (int id) =>
        {
            await using var db = factory.CreateDbContext();
            var p = await db.GuidePictures.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            return p is null ? Results.NotFound() : Results.File(p.Content, p.ContentType);
        });
        app.MapRazorComponents<Verification.VerificationApp>().AddInteractiveServerRenderMode();
        await app.RunAsync();
    }
}
finally
{
    await database.Database.EnsureDeletedAsync();
    Console.WriteLine("Disposable verification database removed.");
}
static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS: " + message);
}
static async Task Reject<T>(Func<Task> operation, string message) where T : Exception
{
    try { await operation(); }
    catch (T) { Console.WriteLine("PASS: " + message); return; }
    throw new Exception(message);
}
sealed class VerificationFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
}
'@ | Set-Content -LiteralPath (Join-Path $fixtureRoot 'Program.cs')
$arguments = @('run', '--project', (Join-Path $fixtureRoot 'Verification.csproj'), '--', $projectRoot)
if ($Serve) { $arguments += '--serve' }
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw 'Vocabulary picture verification failed.' }
