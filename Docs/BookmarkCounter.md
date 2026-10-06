# BookmarkCounter module

This module was brought from `F:\My.Prog\BookmarkCounter\BookmarkCounter\BookmarkCounter` into MyNotes. Its existing `BookmarkTracker` database, `BookmarkCounter.*` C# namespaces, and migrations are retained.

## Features and routes

- `/` (with `/bookmarks` as an alias): live Edge bookmark count, daily/weekly/one-time checklist, missed tasks, and question preview.
- `/bookmarks/history`: history chart, date range and manual count snapshot.
- `/bookmarks/questions`: questions and optional answers with completion status.
- `/bookmarks/api/...`: original count, history, checklist and question endpoints under the module prefix.

The module reads the configured Edge Bookmarks JSON file and counts URL nodes recursively within `YT_Prio`, including subfolders. CountLogger records a snapshot at startup and every hour. ChecklistBackfiller ensures checklist progress rows at startup and every 30 minutes. The application must keep running for these workers to run; closing a browser tab does not stop them.

## Code map

- `Modules/BookmarkCounter/BookmarkModule.cs`: registrations, prefixed endpoint mappings, and request validation.
- `Modules/BookmarkCounter/Data/`: bookmark counts, checklist items/progress, questions and AppDbContext.
- `Modules/BookmarkCounter/Services/`: reader, repositories and background workers.
- `Modules/BookmarkCounter/Migrations/`: preserved migration IDs and snapshot.
- `wwwroot/bookmarks/`: HTML screens, CSS and JavaScript. These remain static pages served by the shared host.

## Configuration and maintenance

`ConnectionStrings:BookmarkCounter` points to BookmarkTracker. `Bookmarks:Folder` and `Bookmarks:EdgeProfilePath` retain the original settings. `Bookmarks:EnableBackgroundTasks` can disable both scheduled workers when another host is responsible for them. Avoid running this module's workers and the standalone BookmarkCounter workers simultaneously.

All fetch calls use `/bookmarks/api/...`, and all module links/assets use `/bookmarks/...`. The browser sends recurrence values as enum names; the shared host configures the JSON enum converter. Preserve the request validation and HTTP methods when extending endpoints.

The data timestamps follow the original conventions: bookmark snapshots are stored in UTC, while checklist periods use local calendar dates. History charts currently use Chart.js from a CDN, as in the original app. The Edge file is accessed by the server process account, so a future Windows Service installation must use an account that can read it.

Schema changes use `--context BookmarkCounter.Data.AppDbContext --output-dir Modules/BookmarkCounter/Migrations`. Update this guide for feature or configuration changes. Verify API reads and a disposable CRUD record when changing a write endpoint, then remove that test record.

The dashboard is the application home page. Its public cross-module navigation contains only the `/journal` link. Do not expose a MovieReview link in these HTML screens.

Use `getElement` for element lookups in the bookmark scripts. Do not declare a global `$` helper: Visual Studio Browser Link expects `$` to be jQuery and calls `$.noConflict()`. Bump the script query version in its HTML page when updating these assets.

Both background workers handle `OperationCanceledException` only when their host stopping token is cancelled. This makes normal shutdown/restart complete cleanly instead of appearing as a user-unhandled exception in Visual Studio. Preserve this filtered catch around the timer loop.
