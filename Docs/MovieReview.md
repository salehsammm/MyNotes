# MovieReview module

This module was brought from `F:\My.Prog\MovieReview\MovieReviews\MovieReviews` into the MyNotes host. Its C# and Razor namespaces remain `MovieReviews.*`, and its existing `MovieReviews` database and migration history are retained.

## Features and routes

All original pages use the `/moviereview` prefix: overview, scenes, scene detail/edit/new, performers, performer detail/edit/new, watchlist, studios, studio detail, stats, trophies, ideas, notes, note editor, research, research editor/view, guide, and options.

Scene reviews support drafts, pinned drafts, saved playback position, performer associations and overrides, ratings, tags, locations and positions. Performer features include archive/watchlist flags and attribute details. Notes render Markdown with Markdig. Trophy text import remains available through `dotnet run -- --import-trophies <file>` in the MyNotes project.

## Review import from Re.txt

On October 6, 2026, reviews from `F:\Tala\1\z1\Re.txt` were imported directly into the existing `MovieReviews` database. The database had 7 scenes beforehand; 184 were added, leaving 191 scenes. Two reviews whose titles already existed were skipped. The blank example templates at the start of the file and the trailing numbered site list were not treated as reviews. The source file was not changed.

The import mapped overall, setup, and sex ratings and modifiers to their scene fields. Review text was kept in the corresponding verdict or notes fields, and recognizable positions, locations, and POV flags were recorded. Named performers were linked to existing performer records with their review-specific descriptions in `ScenePerformer.Notes`. Descriptions without a reliable performer name were kept in the scene's sex notes instead of being assigned to someone by guesswork.

Reviews needing placeholder titles use `noname1` through `noname10` in source order. The second review labeled `Dolly Rud - Stepsis got stood up on her date - Family` has different performers from the first and was saved as `noname1` at the user's direction; the other placeholders cover untitled reviews. Do not treat these placeholders as duplicate reviews or replace them automatically; rename one only after its real title is known. For future untitled reviews, use the next unused `noname` number and check existing scene titles before inserting. `All It Took Was A Dare` had performer details but no setup or sex review, so it was saved as a draft with those performer links.

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

## Password protection

Enter through `/private`, which is not shown in public navigation. All pages import the `PrivateArea` authorization attribute. The host gates direct private URLs and assets, and overrides the module DbContext factory with `PrivateMovieDbContextFactory` to check session access before each database operation. Preserve these controls for new pages and services; a hidden link by itself does not enforce privacy.

The module layout includes an antiforgery-protected **Lock private area** form and loads its keyboard shortcut script only inside the private module. The global public App component must not load this script. See root PROJECT.md for password hash storage, session expiry and server maintenance access.
