# BookmarkCounter module

This module was brought from `F:\My.Prog\BookmarkCounter\BookmarkCounter\BookmarkCounter` into MyNotes. Its existing `BookmarkTracker` database, `BookmarkCounter.*` C# namespaces, and migrations are retained.

## Features and routes

- `/` (with `/bookmarks` as an alias): live Edge bookmark count, recent checklist completions with a selectable time range (newest first), daily/weekly/one-time checklist, missed tasks, and question preview. Recently done defaults to the last 2 days; options range from 30 minutes to all time, and the browser remembers the selection. Completion dates and times are shown in local time. `/bookmarks/api/checklist/recent?minutes=2880` accepts a lookback in minutes (0 for all time; maximum 525600), defaulting to 2880.
- `/bookmarks/history`: history chart, date range and manual count snapshot. Minimum and maximum counts summarize the daily points shown in the selected 7-, 30-, or 90-day range and refresh when switching ranges or logging a count. Both show a dash while loading, for an empty range, or when loading fails.
- `/bookmarks/questions`: questions and optional answers with completion status.
- `/bookmarks/api/...`: original count, history, checklist and question endpoints under the module prefix.

The module reads the configured Edge Bookmarks JSON file and counts URL nodes recursively within `YT_Prio`, including subfolders. CountLogger records a snapshot at startup and every hour. ChecklistBackfiller ensures checklist progress rows at startup and every 30 minutes. The application must keep running for these workers to run; closing a browser tab does not stop them.

Recently done is a compact disclosure that starts closed on each page load. Open it to load completion history; automatic refreshes only fetch that history while it is open.

Each Recently done entry has an Undo button. It reverses the final count increment for that exact occurrence (keeping earlier counts), clears its completion timestamp, and refreshes the checklist, missed tasks, and recent history. Past required occurrences return to Missed tasks; other occurrences can be viewed on their original date. The recent API includes `progressId` so Undo uses the existing progress decrement endpoint.

Missed tasks has a **Done all** action with confirmation. It completes all currently missed required occurrences to their target counts in one save, excluding optional tasks, future/current occurrences, and obsolete one-time schedules. Each completion appears in Recently done and can be reopened there.

Task creation and editing include an optional icon (movie, music, bookmark, TV/series, book, check, star, or exercise) and a custom background color with a live preview. These settings belong to the task and appear on daily, weekly, one-time, missed, and recently completed occurrences. Choose None or turn off Use custom color to restore the defaults. Text contrast adapts to the background. The API accepts an allowed icon name or null and a six-digit hex background color or null.

The dashboard question preview shows up to four questions, prioritizing unanswered questions and filling any remaining slots with answered questions.

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

The dashboard is the application home page. Every bookmark page has a sticky two-level header: links to BookmarkCounter, MyNotes journal, and MovieReview on the first row, followed by bookmark navigation. The selected project and section are highlighted. MovieReview still requires unlocking. The footer stays at the bottom of the viewport, with content space reserved above it.

Use `getElement` for element lookups in the bookmark scripts. Do not declare a global `$` helper: Visual Studio Browser Link expects `$` to be jQuery and calls `$.noConflict()`. Bump the script query version in its HTML page when updating these assets.

Both background workers handle `OperationCanceledException` only when their host stopping token is cancelled. This makes normal shutdown/restart complete cleanly instead of appearing as a user-unhandled exception in Visual Studio. Preserve this filtered catch around the timer loop.
