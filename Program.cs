using MyNotes.Components;
using MyNotes.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using MyNotes.Modules.MovieReview;
using MyNotes.Modules.BookmarkCounter;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using MyNotes.Security;
using System.Threading.RateLimiting;

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
builder.Services.AddSingleton<PrivateAccessStore>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "MyNotes.PrivateAccess";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/private";
    options.AccessDeniedPath = "/private";
    options.ExpireTimeSpan = PrivateAccessStore.SessionLifetime;
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = context =>
    {
        if (!context.HttpContext.RequestServices.GetRequiredService<PrivateAccessStore>().IsUnlocked(context.Principal!))
            context.RejectPrincipal();
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization(options => options.AddPolicy("PrivateArea", policy =>
    policy.RequireAuthenticatedUser().RequireClaim(PrivateAccessStore.SessionClaim)));
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PrivateAuthenticationStateProvider>();
if (!(args.Length == 2 && args[0] == "--import-trophies"))
    builder.Services.AddScoped<IDbContextFactory<MovieReviews.Data.AppDbContext>, PrivateMovieDbContextFactory>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("private-login", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

var app = builder.Build();

// Apply checked-in EF Core migrations before serving requests.
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<MediaDbContext>>().CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}
// Schema maintenance runs locally before requests, outside the protected UI factory.
await using (var db = new MovieReviews.Data.AppDbContext(app.Services.GetRequiredService<DbContextOptions<MovieReviews.Data.AppDbContext>>()))
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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/moviereview"))
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.RequestServices.GetRequiredService<PrivateAccessStore>().IsUnlocked(context.User))
        {
            context.Response.Redirect("/private?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
            return;
        }
    }
    await next(context);
});
app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapBookmarkModule();
app.MapPrivateAccess();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
