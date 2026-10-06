using BookmarkCounter.Data;
using BookmarkCounter.Services;
using Microsoft.EntityFrameworkCore;

namespace MyNotes.Modules.BookmarkCounter;

public static class BookmarkModule
{
    public static IServiceCollection AddBookmarkModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BookmarkCounter")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:BookmarkCounter");
        var bookmarksPath = configuration["Bookmarks:EdgeProfilePath"]
            ?? throw new InvalidOperationException("Missing Bookmarks:EdgeProfilePath");

        services.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlServer(connectionString));
        services.AddSingleton<HistoryRepository>();
        services.AddSingleton(new BookmarkReader(bookmarksPath));
        services.AddSingleton<ChecklistRepository>();
        services.AddSingleton<QuestionRepository>();
        if (configuration.GetValue("Bookmarks:EnableBackgroundTasks", true))
        {
            services.AddHostedService<CountLogger>();
            services.AddHostedService<ChecklistBackfiller>();
        }

        return services;
    }

    public static void MapBookmarkModule(this WebApplication app)
    {
        var folderName = app.Configuration["Bookmarks:Folder"] ?? "YT_Prio";
        var group = app.MapGroup("/bookmarks");
        app.MapGet("/", (IWebHostEnvironment env) =>
            Results.File(Path.Combine(env.WebRootPath, "bookmarks", "index.html"), "text/html; charset=utf-8"));
        group.MapGet("/", (IWebHostEnvironment env) =>
            Results.File(Path.Combine(env.WebRootPath, "bookmarks", "index.html"), "text/html; charset=utf-8"));
        group.MapGet("/history", (IWebHostEnvironment env) =>
            Results.File(Path.Combine(env.WebRootPath, "bookmarks", "history.html"), "text/html; charset=utf-8"));
        group.MapGet("/questions", (IWebHostEnvironment env) =>
            Results.File(Path.Combine(env.WebRootPath, "bookmarks", "questions.html"), "text/html; charset=utf-8"));

        group.MapGet("/api/count", (BookmarkReader reader) =>
        {
            var count = reader.CountFolder(folderName);
            return Results.Ok(new { folder = folderName, count, at = DateTime.Now });
        });

        group.MapGet("/api/history", async (HistoryRepository repo, int? days) =>
        {
            var rows = await repo.GetHistoryAsync(folderName, days ?? 30);
            return Results.Ok(rows);
        });

        group.MapPost("/api/log", async (HistoryRepository repo, BookmarkReader reader) =>
        {
            var count = reader.CountFolder(folderName);
            await repo.InsertAsync(folderName, count);
            return Results.Ok(new { count, at = DateTime.Now });
        });

        group.MapGet("/api/checklist", async (ChecklistRepository repo, string? date) =>
        {
            var d = DateOnly.TryParse(date, out var parsed)
                ? parsed : DateOnly.FromDateTime(DateTime.Now);
            return Results.Ok(await repo.GetForDateAsync(d));
        });

        group.MapGet("/api/checklist/missed", async (ChecklistRepository repo) =>
            Results.Ok(await repo.GetMissedAsync()));

        group.MapPost("/api/checklist", async (ChecklistRepository repo, ChecklistRequest req) =>
            ValidChecklistRequest(req) ? Results.Ok(await repo.CreateAsync(req)) : Results.BadRequest());

        group.MapPut("/api/checklist/{id}", async (int id, ChecklistRepository repo, ChecklistRequest req) =>
            !ValidChecklistRequest(req) ? Results.BadRequest() :
            await repo.UpdateAsync(id, req) ? Results.Ok() : Results.NotFound());

        group.MapDelete("/api/checklist/{id}", async (int id, ChecklistRepository repo) =>
            await repo.DeleteAsync(id) ? Results.Ok() : Results.NotFound());

        group.MapPost("/api/checklist/progress/{progressId}/increment", async (int progressId, ChecklistRepository repo) =>
        {
            var dto = await repo.IncrementAsync(progressId);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        group.MapPost("/api/checklist/progress/{progressId}/decrement", async (int progressId, ChecklistRepository repo) =>
        {
            var dto = await repo.DecrementAsync(progressId);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        group.MapGet("/api/questions", async (QuestionRepository repo) => Results.Ok(await repo.GetAllAsync()));
        group.MapPost("/api/questions", async (QuestionRepository repo, QuestionRequest req) =>
            ValidQuestionRequest(req) ? Results.Ok(await repo.CreateAsync(req)) : Results.BadRequest());
        group.MapPut("/api/questions/{id}", async (int id, QuestionRepository repo, QuestionRequest req) =>
            !ValidQuestionRequest(req) ? Results.BadRequest() :
            await repo.UpdateAsync(id, req) ? Results.Ok() : Results.NotFound());
        group.MapDelete("/api/questions/{id}", async (int id, QuestionRepository repo) =>
            await repo.DeleteAsync(id) ? Results.Ok() : Results.NotFound());

    }

    private static bool ValidChecklistRequest(ChecklistRequest req) =>
        !string.IsNullOrWhiteSpace(req.Title) && req.Title.Trim().Length <= 200
        && req.TargetCount is >= 1 and <= 999
        && req.RepeatEveryDays is >= 1 and <= 365
        && Enum.IsDefined(req.Recurrence);

    private static bool ValidQuestionRequest(QuestionRequest req) =>
        !string.IsNullOrWhiteSpace(req.Text) && req.Text.Trim().Length <= 500
        && (req.Answer is null || req.Answer.Trim().Length <= 4000);

}
