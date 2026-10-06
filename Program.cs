using MyNotes.Components;
using MyNotes.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using MyNotes.Modules.MovieReview;
using MyNotes.Modules.BookmarkCounter;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : null
});
builder.Host.UseWindowsService();
// Support local runs without a launch profile as well as published deployments.
builder.WebHost.UseStaticWebAssets();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<MediaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MediaJournal")));
builder.Services.AddMovieReviewModule(builder.Configuration);
builder.Services.AddBookmarkModule(builder.Configuration);

var app = builder.Build();

// Apply checked-in EF Core migrations before serving requests.
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<MediaDbContext>>().CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<MovieReviews.Data.AppDbContext>>().CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<BookmarkCounter.Data.AppDbContext>>().CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}

if (args.Length == 2 && args[0] == "--import-trophies")
{
    await using var scope = app.Services.CreateAsyncScope();
    var trophies = scope.ServiceProvider.GetRequiredService<MovieReviews.Services.TrophyService>();
    var result = await trophies.ImportTextAsync(await File.ReadAllTextAsync(args[1]));
    Console.WriteLine($"Imported {result.Categories} categories and {result.Entries} entries.");
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapBookmarkModule();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
