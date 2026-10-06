# MyNotes combined application

MyNotes is the single .NET 9 host for three personal modules. Run `dotnet run` from this project; there is one ASP.NET Core process and one local address.

| Module | Entry route | Development guide | Database |
| --- | --- | --- | --- |
| MyNotes movie/TV journal | `/journal` | [MyNotes.md](Docs/MyNotes.md) | MyNotesMediaJournal |
| MovieReview collection (private) | `/private` unlocks `/moviereview` | [MovieReview.md](Docs/MovieReview.md) | MovieReviews |
| BookmarkCounter (default dashboard) | `/` (`/bookmarks` also works) | [BookmarkCounter.md](Docs/BookmarkCounter.md) | BookmarkTracker |

## Architecture

`Program.cs` registers all three modules and applies their EF migrations before starting the web host. They retain separate DbContexts, migration histories, namespaces, and databases. Their existing records are used directly. The original application folders remain as references; ongoing development of the combined app belongs here.

- Journal: `Data/`, `Migrations/`, `Components/Pages/Home.razor`.
- MovieReview: `Modules/MovieReview/` and `wwwroot/moviereview/`.
- BookmarkCounter: `Modules/BookmarkCounter/` and `wwwroot/bookmarks/`.
- Public links: `Components/ModuleNavigation.razor` links to the dashboard and journal. Bookmark static pages show only the journal link alongside their usual internal navigation. MovieReview is not linked publicly.
- Private access: `Security/` contains password verification, cookie/session handling, Blazor authentication state, and the protected MovieReview database factory.

All packages and compiled Razor components belong to `MyNotes.csproj`. Root `Components/App.razor` owns the HTML document, Blazor script and combined scoped CSS bundle. Each Blazor layout loads its own global styles. Module links perform full navigation to keep the existing styles separate. The existing module screens are retained; the common UI/UX is pending a later design discussion.

## Configuration and background work

`appsettings.json` holds separate `MediaJournal`, `MovieReview`, and `BookmarkCounter` connection strings, plus the existing `Bookmarks` settings. SQL Server must be reachable and the running Windows account must have access to all three databases and the Edge profile file.

`Bookmarks:EnableBackgroundTasks` defaults to true. It controls bookmark count logging and checklist backfill. These run for the lifetime of MyNotes, even when the browser is closed. Avoid running the old BookmarkCounter host at the same time, which would duplicate scheduled work. Windows Service hosting is supported, but the merge does not install a service or configure automatic startup.

## Development

Build with `dotnet build`, then start and verify the affected module. Update its guide when changing behavior, and this guide when changing shared architecture. Check route prefixes, static asset URLs, and navigation after copying or adding pages. Do not share or rename module entities just because their names overlap.

For migrations, always specify the context and output directory:

```powershell
dotnet ef migrations add ChangeName --context MyNotes.Data.MediaDbContext --output-dir Migrations
dotnet ef migrations add ChangeName --context MovieReviews.Data.AppDbContext --output-dir Modules/MovieReview/Migrations
dotnet ef migrations add ChangeName --context BookmarkCounter.Data.AppDbContext --output-dir Modules/BookmarkCounter/Migrations
```

Install the matching EF 9 CLI tool if needed. Review migrations before starting the app, because startup applies pending migrations to all three databases. Keep personal data out of migration seed code. The private module is password protected; this remains a personal local application.

## Merge verification

The combined project built with zero warnings/errors. All three migration histories were recognized as up to date. The journal retained 40 titles/180 episodes; MovieReview retained 7 scenes, 352 performers, and 8 notes. Main module routes, MovieReview lists and new-entry editors, and BookmarkCounter API reads returned successfully. A disposable MovieReview note was saved through the browser and a disposable BookmarkCounter question was created/read/updated through its API; both were removed afterward. Browser checks confirmed module switching, the live bookmark count/history chart, and the journal's IMDb filter. Bookmark logging at startup was observed.

## Private access

Open `/private` manually to unlock MovieReview. It is absent from public navigation. The owner password is stored as a randomly salted PBKDF2-SHA256 hash with 600,000 iterations at `%LOCALAPPDATA%\MyNotes\private-access.json`, outside the repository. Never commit or log the password. `PrivateAccess:PasswordFile` can override the file path if the app runs under another Windows account. A missing password file leaves the private area locked.

Successful entry creates an HttpOnly, SameSite Strict session cookie and an in-memory session with a 30-minute absolute lifetime. The **Lock private area** button revokes the session and returns to the dashboard. Restarting the app also invalidates sessions. Incorrect passwords are rejected, login attempts are limited to five per minute, and unlock/lock forms require antiforgery tokens.

MovieReview pages require the `PrivateArea` authorization policy. Middleware also gates its static assets and direct URLs. The scoped private database factory checks the live session before each MovieReview database operation, so an already-open Blazor circuit cannot continue reading/writing after its session is revoked. Authentication state is revalidated every ten seconds. Startup migrations and the explicit local trophy-import command use trusted host access outside this UI gate.

Privacy verification covered the public dashboard/journal, direct private page and asset access, incorrect passwords, missing antiforgery tokens, correct login and protected database reads, logout, and old-cookie rejection. Browser checks confirmed the public navigation and private editor keyboard shortcut.

The word **PERSONAL** in the journal header is a discreet link to `/private`. It performs a full navigation to the password screen (or the private area when the current session is already unlocked). Keep it styled like the surrounding caption, with a visible keyboard focus outline.
