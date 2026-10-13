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
- `SavedToImdb` is a checkbox on each title; `SavedToLetterboxd` is shown only for movies, including its library indicator. The library has a **Not on IMDb** filter.
- `Episode` belongs to a series and stores season/episode numbers, optional title, optional rating, and freeform Unicode notes or review.
- **Add season** on a saved series creates episodes 1 through the chosen count in one save (default 10). The season defaults to the next season and can be changed to fill an existing one. Existing episodes, ratings, notes and episode 0 specials are preserved; only missing episodes are added, with blank ratings and notes. Seasons accept 1–999 and episode counts 1–9999.
- When editing a saved series, episodes appear immediately below the title and type. Episode names are hidden; rows show season/episode numbers and optional notes. Click a rating to edit it directly; Enter, Tab, or leaving the field saves it (blank clears it). Episode details and new episodes use a small dialog with optional notes. Existing stored episode names are preserved.
- A null rating means **not rated yet** (`?` in the sample notes). Ratings can have one decimal place and must be between 0 and 10. Seasons start at 1; episode 0 is allowed for a special. Each season/episode pair is unique within a series.
- Deleting a series deletes its episodes. Changing a series into a movie should be done only after its episodes are removed; the UI should preserve those notes until the user explicitly deletes them.

On 2026-10-06, the user-provided desktop note file was imported directly into the local database: 40 titles and 180 episodes. The file is not copied into the repository or seeded in migrations. A title beginning with `**` was marked as saved to both IMDb and Letterboxd; other titles remain unchecked even if the note mentions those sites. The earlier unlabelled `Silo` episode list conflicts with a later explicit season 1 list, so it is preserved in the series review while the explicit season lists supply episode rows. `Win or Lose` lists E07 twice with different ratings; the first value is the episode rating and the second is preserved in the series review. Unassigned reminders and trailing `E01`–`E24` placeholders were not interpreted as ratings.

## Personal notes and diary

`/journal/notes` is linked from the journal toolbar. Personal notes have a title, note date, optional custom category, and separate Details, My understanding & conclusion, and Summary sections. Diary entries start with today's date in the Asia/Tehran time zone and a suggested title; their freeform entry is prominent, with conclusion and summary optional. Multiple entries per day are allowed. Dates remain editable for past entries.

Search combines all words, any word, or an exact phrase with section, type, category, and inclusive note-date filters. Sort by note date, most recent edit, or title; results load 50 at a time. Text is stored as Unicode without stripping paragraphs. Creation/edit timestamps are stored in UTC and displayed in Tehran time. Switching entries or leaving the page warns about unsaved changes; notes use explicit Save. This section has the same access as the media journal.

`Data/PersonalNote.cs`, `Components/Pages/PersonalNotes.razor` and `.razor.cs` implement the first version, backed by the existing journal database and an additive migration. The three sections can be extended after reviewing real examples of the user's writing.

Notes can optionally belong to a named group such as “Limitless Book” under Life. Each point is a separate topic/note with its own details, conclusion, summary, and ordered research entries. Research entries label Website, AI, or Other, with optional source name and an HTTP(S) link. Research text, source names, and links are included in word/phrase search, or searched separately using the Research & sources scope. Search can be limited to one group, ungrouped notes, or all notes, combined with category, date, and type filters.

A grouped note uses its group's category; group renaming or recategorization does not rewrite its writing. Existing notes remain ungrouped until moved with the Note group selector. Groups can only be deleted when empty; topics must first be moved out. Research is saved with its topic, and deleting a topic deletes its research entries. Removing research while editing is pending until Save. Diaries remain freeform and ungrouped by default.

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
