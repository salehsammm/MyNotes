using MyNotes.Components;
using MyNotes.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<MediaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MediaJournal")));

var app = builder.Build();

// Apply checked-in EF Core migrations before serving requests.
await using (var db = await app.Services.GetRequiredService<IDbContextFactory<MediaDbContext>>().CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
