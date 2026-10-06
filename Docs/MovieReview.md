# MovieReview module

This module was brought from `F:\My.Prog\MovieReview\MovieReviews\MovieReviews` into the MyNotes host. Its C# and Razor namespaces remain `MovieReviews.*`, and its existing `MovieReviews` database and migration history are retained.

## Features and routes

All original pages use the `/moviereview` prefix: overview, scenes, scene detail/edit/new, performers, performer detail/edit/new, watchlist, studios, studio detail, stats, trophies, ideas, notes, note editor, research, research editor/view, guide, and options.

Scene reviews support drafts, pinned drafts, saved playback position, performer associations and overrides, ratings, tags, locations and positions. Performer features include archive/watchlist flags and attribute details. Notes render Markdown with Markdig. Trophy text import remains available through `dotnet run -- --import-trophies <file>` in the MyNotes project.

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
