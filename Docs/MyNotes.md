# Frame Notes project guide

Frame Notes is the movie/TV journal module in the combined MyNotes host, built with .NET 9 Blazor Server, Entity Framework Core 9, and SQL Server. It lives at `/journal`. See the root `PROJECT.md` for shared hosting and the other modules.

## Run it

1. Start the local SQL Server instance `.\MSSQLSERVER2` and ensure your Windows account can create a database there.
2. Run `dotnet run` from this directory.
3. Open the local URL printed in the terminal.

The connection string is in `appsettings.json`. It uses Windows authentication and database `MyNotesMediaJournal`. Before choosing it, the existing SQL Server databases were checked; `MovieReviews` already exists and is intentionally untouched. For another computer, change `ConnectionStrings:MediaJournal` in local configuration or an environment variable rather than changing code.

At startup, `Program.cs` calls `Database.MigrateAsync()`. This creates the database if needed and applies pending checked-in EF migrations. The app needs a working SQL Server connection to start. Add a new migration whenever the EF model changes; startup migration applies it on the next run. Do not use `EnsureCreated`, which bypasses migrations.

## Data model

- `MediaItem` represents either a movie or a series. It stores the title, optional release year, optional overall rating out of 10, and freeform Unicode review text.
- `SavedToImdb` and `SavedToLetterboxd` are independent checkboxes on each title. The library has a **Not on IMDb** filter.
- `Episode` belongs to a series and stores season/episode numbers, optional title, optional rating, and freeform Unicode notes or review.
- A null rating means **not rated yet** (`?` in the sample notes). Ratings can have one decimal place and must be between 0 and 10. Seasons start at 1; episode 0 is allowed for a special. Each season/episode pair is unique within a series.
- Deleting a series deletes its episodes. Changing a series into a movie should be done only after its episodes are removed; the UI should preserve those notes until the user explicitly deletes them.

On 2026-10-06, the user-provided desktop note file was imported directly into the local database: 40 titles and 180 episodes. The file is not copied into the repository or seeded in migrations. A title beginning with `**` was marked as saved to both IMDb and Letterboxd; other titles remain unchecked even if the note mentions those sites. The earlier unlabelled `Silo` episode list conflicts with a later explicit season 1 list, so it is preserved in the series review while the explicit season lists supply episode rows. `Win or Lose` lists E07 twice with different ratings; the first value is the episode rating and the second is preserved in the series review. Unassigned reminders and trailing `E01`–`E24` placeholders were not interpreted as ratings.

## Code map

- `Data/MediaItem.cs`, `Data/Episode.cs`: domain records and input validation.
- `Data/MediaDbContext.cs`: EF relationships, indexes, SQL constraints.
- `Migrations/`: versioned schema changes.
- `Components/Pages/Home.razor`: journal UI and save/delete actions.
- `wwwroot/app.css`: visual styling.
- `Program.cs`: service registration and automatic migration.

## Development workflow

1. Edit the entity classes and/or `MediaDbContext`.
2. Create a migration with `dotnet ef migrations add DescriptiveName --context MyNotes.Data.MediaDbContext --output-dir Migrations` (install the matching EF Core 9 CLI tool if needed).
3. Review the generated migration, then run `dotnet build` and `dotnet run`.
4. Check that existing reviews remain intact after migrations. Do not edit an already applied migration; create a new one.

Use parameterized EF queries, keep all review content as Unicode, and never put personal review data into migration seed code. This is a single-user local app; it does not yet have account authentication or bulk import.

The word **PERSONAL** in the journal header is a discreet link to `/private`. It performs a full navigation to the password screen (or the private area when the current session is already unlocked). Keep it styled like the surrounding caption, with a visible keyboard focus outline.
