# MovieReview module

This module was brought from `F:\My.Prog\MovieReview\MovieReviews\MovieReviews` into the MyNotes host. Its C# and Razor namespaces remain `MovieReviews.*`, and its existing `MovieReviews` database and migration history are retained.

## Features and routes

All original pages use the `/moviereview` prefix: overview, scenes, scene detail/edit/new, performers, performer detail/edit/new, watchlist, studios, studio detail, stats, trophies, ideas, notes, note editor, research, research editor/view, guide, and options.

Scene reviews support drafts, pinned drafts, saved playback position, performer associations and overrides, ratings, tags, locations and positions. Performer features include archive/watchlist flags and attribute details. Notes render Markdown with Markdig. Trophy text import remains available through `dotnet run -- --import-trophies <file>` in the MyNotes project.

## Review import from Re.txt

On October 6, 2026, reviews from `F:\Tala\1\z1\Re.txt` were imported directly into the existing `MovieReviews` database. The database had 7 scenes beforehand; 184 were added, leaving 191 scenes. Two reviews whose titles already existed were skipped. The blank example templates at the start of the file and the trailing numbered site list were not treated as reviews. The source file was not changed.

The import mapped overall, setup, and sex ratings and modifiers to their scene fields. Review text was kept in the corresponding verdict or notes fields, and recognizable positions, locations, and POV flags were recorded. Named performers were linked to existing performer records with their review-specific descriptions in `ScenePerformer.Notes`. Descriptions without a reliable performer name were kept in the scene's sex notes instead of being assigned to someone by guesswork.

Reviews needing placeholder titles use `noname1` through `noname10` in source order. The second review labeled `Dolly Rud - Stepsis got stood up on her date - Family` has different performers from the first and was saved as `noname1` at the user's direction; the other placeholders cover untitled reviews. Do not treat these placeholders as duplicate reviews or replace them automatically; rename one only after its real title is known. For future untitled reviews, use the next unused `noname` number and check existing scene titles before inserting. `All It Took Was A Dare` had performer details but no setup or sex review, so it was saved as a draft with those performer links.

## Raindrop review import

On October 7, 2026, 56 ZIP exports in `F:\Tala\1\z1\ForCodex` supplied 676 bookmark rows. After comparing URLs, titles, URL slugs, and substantial review text, 492 scenes were added and 181 existing scenes were enriched. The database then had 683 scenes, 673 with a primary URL. One scene has a second source URL in `AlternateUrls`. All 676 original export rows are preserved in the JSON `SourceData` column, including two repeated URLs and the alternate URL. No exact duplicate scene titles or primary URLs were found after import. `dide (111).zip` supplied two previously imported reviews; `dide (49).zip` arrived later and supplied 24 additional `Cum Swapping Sis` reviews. Its misleading "Featured Videos" export title was resolved from its scene URL as `Cum Swapping Sis - Bubble Cum`.

`Scene.Url` is editable and opens from the scene detail page. The original Raindrop title, note, excerpt, cover URL, highlights, bookmark ID, saved date, favorite flag, and source archive are stored in dedicated scene fields; `SourceData` keeps the unmodified export fields for provenance. Existing review text and ratings were retained when already present. New reviews use structured ratings, positions, locations, and POV flags where the note states them. Sparse reviews without a clear rating were saved as drafts. The original note can be expanded on the detail page.

Studios were assigned from clearly branded domains or names in the source title. Generic aggregator domains, including FamilyPornHD, were not treated as studios. Existing studio values were kept. The import created 175 clearly named performer records and 360 new scene-performer links; uncertain labels remain in the original notes for later review.

### Procedure for the next review files

The current source folder is `F:\Tala\1\z1\ForCodex`. Treat these files and `F:\Tala\1\z1\Re.txt` as read-only inputs. There is no persistent import command in this project yet; the previous imports were one-off database operations. Before importing, inventory every new file, its format, row count, filename, and any repeated rows. Compare the inventory with `SourceArchive` and the archive names inside `SourceData`; a newly uploaded ZIP may have arrived after the last import. Do not assume a familiar filename means its contents were processed.

For each source row, use this order:

1. Compare its bookmark ID (`RaindropId`) and normalized URL with existing scenes, including `AlternateUrls` and URLs inside `SourceData`. Ignore harmless URL differences such as trailing slashes and tracking parameters, but keep the exact original URL in the source row.
2. If URLs do not settle the match, compare a cleaned title, URL slug, performers when explicitly named, and substantial review text. An identical or similar title alone is insufficient: different scenes can share a title. If two existing scenes are plausible, hold that row for review rather than guessing.
3. For a clear match, enrich the existing scene. Fill missing URL, cover, studio, bookmark metadata, and review details without overwriting existing user-written ratings or notes. Put a genuinely different scene URL in the JSON `AlternateUrls` array. Append each distinct original export row, with its archive filename, to the JSON `SourceData` array; do not append the same row twice. Keep the first source fields unless a correction is certain.
4. Otherwise create a scene even when the review is sparse. Keep the note in `SourceNote` and preserve the full original row in `SourceData`; mark incomplete reviews as drafts. Extract structured ratings and other fields only when the note supports them. Do not infer performers from a vague description.

Use a reliable scene title from the export or destination URL when possible. An export page title can be generic or wrong: `Cum Swapping Sis - Featured Videos` was resolved from its scene URL as `Cum Swapping Sis - Bubble Cum`. When no usable title can be established, allocate the next unused `noname` number. Keep the source title in `SourceTitle` even when the display title is corrected. Do not merge or rename existing `noname` scenes solely because their titles are placeholders.

Assign `Studio` only from a clear publisher/series brand in the source title or an authoritative studio domain. Leave it blank when unclear; a generic aggregator such as FamilyPornHD is not a studio. Keep any existing studio value unless the source clearly proves it wrong. Preserve source URLs even when they point to aggregators.

Run an import in a transaction and make it safe to rerun. Afterward, reconcile source row counts against rows represented in `SourceData`, check for repeated bookmark IDs and duplicate primary or alternate URLs, inspect ambiguous matches and placeholder titles, and confirm every newly identified file appears in the database. Record the new file names, row counts, scenes added or enriched, total scenes, and unresolved cases in this document. Never recreate the existing `MovieReviews` database to perform an import.

## Review export editing

After completing a review, the export page lets you edit the generated notepad text directly before copying it. These edits apply to the current copy only; use **Edit scene** to persist changes to the review fields. Text exports omit the scene URL and performer age and language flags, except **MILF**, which is included when selected. Setup ratings appear before setup flags and notes, with a blank line separating the opening summary from Setup. Performer blocks include face quality, body size/quality, and moans descriptions with the moans label (including override-only descriptions). Draft exports also include performer blocks.

In the scene editor, **New performer** prefills the name from the current performer search text.

Performer override cards offer independent **Kinda perfect**, **Too great**, and **Goddess** checkboxes. These badges belong to the scene-performer link, persist with that review, and appear in its text export; they never change the performer profile. Reverse can only be selected while Cowgirl is checked; unchecking Cowgirl clears Reverse. Their combined export label is `cowgirl and R`. Older Reverse-only reviews are shown with Cowgirl selected when opened for editing and use the combined export label.

## Archive collections and manual performer rankings

`/moviereview/collections` is a private archive library, separate from scene reviews. On October 17, 2026, all 327 ZIP exports in `F:\Tala\1\z1\aaa\ForCodex2` were imported as 327 collections: 6,046 CSV bookmarks plus two extra text-file records. The complete original ZIP bytes are stored in `ArchiveCollections.OriginalArchive` with a SHA-256 fingerprint. This preserves CSV/HTML/URL exports, 14 PNG attachments, and extra text files independently of the source folder. Keep source files read-only and personal archive bytes out of Git and migration seeds.

Each bookmark retains its title, exact URL, original note and all CSV fields in `SourceData`, including multiline whitespace. Notes, status, priority, saved progress, type and optional scene/performer links are independently editable. Explicit `page`/`p` query values were preserved on 217 bookmarks; folder names such as `Finished`, `Prio`, and `S_Tier` were retained without inventing parent folders, ratings or completion meaning. The 88 groups of repeated exact URLs remain separate source records. Type classification is conservative; uncertain entries remain Bookmark. Series entries remain archived as Series rather than being converted to episode reviews.

Twenty-five bookmarks were linked to existing scenes by bookmark ID or normalized URL. No scenes or existing reviews were modified. `My_Gs.zip` contains 29 performer entries: 16 exact existing names were reused and 13 profiles were added using source labels as written. No alias merges were inferred (for example, `Myra` and `Myra Moans` remain distinct). Original descriptions are available through **Archived notes & links** rather than overwriting profile attributes.

`/moviereview/performer-rankings` provides the owner's manual best-to-worst order. Set a position, move up/down, add to the standings or unrank. My Gs is the initial filter; all performers and all ranked performers are also available. Positions are global and remain unchanged by filters. All performers start unranked; archive ordering is not a preference ranking. Reorder operations run in serializable transactions and compact positions. Performer merges transfer archive links and preserve the surviving ranking; deletion compacts remaining positions.

Archive and attachment downloads are under `/moviereview/collections/...`, require the private policy and a live session, and send `no-store`. Attachments are read from stored ZIP bytes by their recorded entry name, never served from public `wwwroot`. Only HTTP(S) source URLs are clickable. Locking or revoking a session protects pages, circuit database actions, and downloads.

Local maintenance commands (run from the project folder):

```powershell
dotnet run --no-launch-profile -- --import-collections 'F:\Tala\1\z1\aaa\ForCodex2'
dotnet run --no-launch-profile -- --link-my-gs
```

The importer migrates only MovieReview, uses a transaction for the batch, and skips archive hashes already present. Changed archive bytes create a separate collection version. It preserves each distinct bookmark row rather than deduplicating away the archive's data. The local My Gs linker is idempotent and does not set ranking positions. These commands use trusted local host database access; they do not relax the web session gate.

The external audit at `F:\Tala\1\z1\aaa\ForCodex2-audit` contains inventory, fingerprints, source snapshots, reconciliation, import/test logs and a pre-import SQL backup. Verification compares every original ZIP byte and every CSV field, tests rerun safety and formatting with a disposable fixture, checks search/edit/pagination, validates manual ranking and profile merge/delete behavior, and checks live-session protection. `scripts/verify_collections.cs.template` is the reproducible service/import verification harness, intended for a disposable console project under `bin` referencing `MyNotes.csproj`. It restores test edits and removes its own database fixtures.

## Code map

- `Modules/MovieReview/MovieReviewModule.cs`: service registrations and context factory.
- `Modules/MovieReview/Data/`: original domain models and `AppDbContext`.
- `Modules/MovieReview/Services/`: original scoped services.
- `Modules/MovieReview/Migrations/`: original migration IDs and model snapshot.
- `Modules/MovieReview/Components/Pages/`: prefixed Razor routes and navigation.
- `Modules/MovieReview/Components/Layout/`: existing layout and navigation menu.
- `wwwroot/moviereview/`: existing stylesheet, Bootstrap assets, and Ctrl+S/Esc JavaScript.

`Components/_Imports.razor` in this module explicitly sets the Razor namespace. The pages import applies the module layout. MyNotes' Router finds these components in the same application assembly.

## Maintenance rules

Use `/moviereview/...` for links and `NavigationManager.NavigateTo` targets. Every EditForm needs a unique FormName. Add `@key` to reorderable rows. Stop click propagation on buttons within clickable headers. Use a fresh DbContext from the factory for each operation. Keep self-referencing research relationships restricted on delete.

Ctrl+S and Esc handlers are registered by editors and disposed on departure; the script is loaded by the host App component. Module styles are loaded by the module layout. Do not add a second Blazor App component, router, or host Program.cs.

Schema changes use `--context MovieReviews.Data.AppDbContext --output-dir Modules/MovieReview/Migrations`. Connection configuration is `ConnectionStrings:MovieReview`. Update this guide when features or schema change. Verify CRUD on the affected page and that the preserved migration history is used; never recreate the existing database.

EF tooling uses `DesignTimeAppDbContextFactory` to read the same connection configuration outside a Blazor session. Runtime database access continues to use the private session gate. Check generated snapshot paths carefully: the combined host has two contexts named `AppDbContext`, and the MovieReview snapshot belongs under `Modules/MovieReview/Migrations`.

## Password protection

Enter through `/private`, which is not shown in public navigation. All pages import the `PrivateArea` authorization attribute. The host gates direct private URLs and assets, and overrides the module DbContext factory with `PrivateMovieDbContextFactory` to check session access before each database operation. Preserve these controls for new pages and services; a hidden link by itself does not enforce privacy.

The module layout includes an antiforgery-protected **Lock private area** form and loads its keyboard shortcut script only inside the private module. The global public App component must not load this script. See root PROJECT.md for password hash storage, session expiry and server maintenance access.
