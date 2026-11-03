$ErrorActionPreference = 'Stop'
$project = Join-Path (Split-Path -Parent $PSScriptRoot) 'MyNotes.csproj'
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('MyNotes-LockChecks-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><ProjectReference Include="$([System.Security.SecurityElement]::Escape($project))" /></ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $testDirectory 'Checks.csproj')
@'
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MyNotes.Components.Pages;
using MyNotes.Data;
using MyNotes.Security;

const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
var passwordFile = Path.Combine(AppContext.BaseDirectory, "test-password.json");
var salt = RandomNumberGenerator.GetBytes(32);
var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
File.WriteAllText(passwordFile, JsonSerializer.Serialize(new { Salt = Convert.ToBase64String(salt), Hash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, salt, 600000, HashAlgorithmName.SHA256, 32)), Iterations = 600000 }));
var access = new PrivateAccessStore(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["PrivateAccess:PasswordFile"] = passwordFile }).Build());
var auth = new TestAuth();
var options = new DbContextOptionsBuilder<MediaDbContext>().UseSqlServer("Server=.\\MSSQLSERVER2;Database=MyNotesLockChecks_" + Guid.NewGuid().ToString("N") + ";Trusted_Connection=True;TrustServerCertificate=True").Options;
await using var db = new MediaDbContext(options);
var page = new PersonalNotes();
try
{
    await db.Database.MigrateAsync();
    Check(access.VerifyPassword(password) && !access.VerifyPassword("wrong-password"), "Existing password verifier accepts correct password only");
    var normal = new PersonalNote { Title = "Public", NoteDate = new(2026,11,3), Details = "Public writing" };
    var secret = new PersonalNote { Title = "Secret title", Kind = PersonalNoteKind.Diary, NoteDate = normal.NoteDate, Category = "Secret category", Details = "Hidden text\nسلام", IsLocked = true, IsFavorite = true, ResearchEntries = [new() { SourceKind = ResearchSourceKind.Website, Content = "Secret research" }] };
    db.AddRange(normal, secret); await db.SaveChangesAsync();
    Inject("DbFactory", new Factory(options)); Inject("Logger", NullLogger<PersonalNotes>.Instance); Inject("AuthenticationState", auth); Inject("PrivateAccess", access);
    await Call("OnInitializedAsync");
    Check(Get<List<PersonalNote>>("notes").Select(n => n.Id).SequenceEqual(new[]{normal.Id}), "Public library excludes protected entries");
    Check(!Get<List<string>>("categories").Contains(secret.Category!), "Protected categories do not leak");
    Set("searchText", "Secret"); await Call("Search"); Check(Get<int>("resultCount") == 0, "Public text search cannot discover protected entries");
    await Call("ClearSearch"); await Call("FilterFavorites"); Check(Get<int>("resultCount") == 0, "Public favorite filter cannot discover protected entries");
    await Call("ClearSearch"); await Call("RequestSelect", secret.Id); Check(Get<int?>("selectedId") is null, "Direct protected selection is rejected");
    Set("selectedId", (int?)secret.Id); Set("form", new PersonalNote { Title = "Tampered", NoteDate = normal.NoteDate }); await Call("SaveNote");
    db.ChangeTracker.Clear(); Check((await db.PersonalNotes.FindAsync(secret.Id))!.Title == secret.Title, "Stale unauthenticated save cannot change protected writing");
    Set("selectedId", (int?)secret.Id); await Call("DeleteNote"); Check(await db.PersonalNotes.AnyAsync(n => n.Id == secret.Id), "Unauthenticated delete is rejected");
    var token = access.CreateSession(); auth.User = new(new ClaimsIdentity([new Claim(PrivateAccessStore.SessionClaim, token)], "test"));
    await Call("Search"); Check(Get<int>("resultCount") == 2, "Private session reveals protected entries");
    await Call("RequestSelect", secret.Id); Check(Get<PersonalNote>("form").ResearchEntries.Single().Content == "Secret research", "Authenticated entry loads its research");
    await Call("ToggleLock"); Check(!Get<PersonalNote>("form").IsLocked && Get<bool>("dirty"), "Lock icon marks protection changes unsaved");
    access.Revoke(auth.User); await Call("RefreshAccess");
    Check(Get<int?>("selectedId") is null && Get<PersonalNote>("form").Details is null, "Revocation clears an open protected entry even with an unsaved unlock");
    Check(Get<List<PersonalNote>>("notes").All(n => !n.IsLocked), "Revocation removes protected library content");
    token = access.CreateSession(); auth.User = new(new ClaimsIdentity([new Claim(PrivateAccessStore.SessionClaim, token)], "test"));
    await Call("RequestSelect", secret.Id);
    var sessions = (System.Collections.Concurrent.ConcurrentDictionary<string,DateTimeOffset>)typeof(PrivateAccessStore).GetField("sessions", Flags)!.GetValue(access)!;
    sessions[token] = DateTimeOffset.UtcNow.AddSeconds(-1);
    await Call("RefreshAccess"); Check(Get<int?>("selectedId") is null && !access.IsUnlocked(auth.User), "Expired sessions clear protected writing");
    token = access.CreateSession(); auth.User = new(new ClaimsIdentity([new Claim(PrivateAccessStore.SessionClaim, token)], "test"));
    await Call("RequestSelect", secret.Id); await Call("ToggleLock"); await Call("SaveNote");
    db.ChangeTracker.Clear(); Check(!(await db.PersonalNotes.FindAsync(secret.Id))!.IsLocked, "Authenticated owner can remove protection");
    access.Revoke(auth.User); await Call("Search"); await Call("RequestSelect", secret.Id);
    Check(Get<PersonalNote>("form").Details == "Hidden text\nسلام", "Unprotected entry becomes readable without losing text");
    await Call("ToggleLock"); await Call("SaveNote");
    Check(Get<int?>("selectedId") is null && Get<List<PersonalNote>>("notes").All(n => n.Id != secret.Id), "Saving protection outside private session immediately hides writing");
    await Call("RequestSelect", normal.Id);
    db.ChangeTracker.Clear(); var other = await db.PersonalNotes.FindAsync(normal.Id); other!.IsLocked = true; await db.SaveChangesAsync();
    await Call("RefreshAccess"); Check(Get<int?>("selectedId") is null, "Protection changed in another page clears stale public editor");
    var safeReturn = typeof(PrivateAccessEndpoints).GetMethod("SafeReturnUrl", BindingFlags.Static | BindingFlags.NonPublic)!;
    Check((string)safeReturn.Invoke(null, ["/journal/notes"])! == "/journal/notes" && (string)safeReturn.Invoke(null, ["https://example.com"])! == "/moviereview", "Unlock returns to notes and rejects external destinations");
    await using var staleDb = new MediaDbContext(options);
    var staleNote = await staleDb.PersonalNotes.FindAsync(normal.Id);
    db.ChangeTracker.Clear(); var freshNote = await db.PersonalNotes.FindAsync(normal.Id); freshNote!.IsLocked = false; await db.SaveChangesAsync();
    staleNote!.Title = "Concurrent stale edit";
    try { await staleDb.SaveChangesAsync(); throw new Exception("Stale update overwrote a changed lock"); }
    catch (DbUpdateConcurrencyException) { Check(true, "Concurrent lock changes reject stale updates"); }
    Console.WriteLine("All note privacy checks passed.");
}
finally
{
    await page.DisposeAsync();
    await db.Database.EnsureDeletedAsync();
    File.Delete(passwordFile);
    Console.WriteLine("Temporary database and synthetic password record removed.");
}
void Inject(string name, object value) => typeof(PersonalNotes).GetProperty(name, Flags)!.SetValue(page,value);
T Get<T>(string name) => (T)typeof(PersonalNotes).GetField(name, Flags)!.GetValue(page)!;
void Set(string name, object? value) => typeof(PersonalNotes).GetField(name, Flags)!.SetValue(page,value);
async Task Call(string name, params object[] args) { if(typeof(PersonalNotes).GetMethod(name,Flags)!.Invoke(page,args) is Task task) await task; }
void Check(bool passed, string message) { if(!passed) throw new Exception(message); Console.WriteLine("PASS: " + message); }
class Factory(DbContextOptions<MediaDbContext> options) : IDbContextFactory<MediaDbContext> { public MediaDbContext CreateDbContext() => new(options); }
class TestAuth : AuthenticationStateProvider { public ClaimsPrincipal User = new(new ClaimsIdentity()); public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(User)); }
'@ | Set-Content -LiteralPath (Join-Path $testDirectory 'Program.cs')
dotnet run --project (Join-Path $testDirectory 'Checks.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Note privacy verification failed.' }
