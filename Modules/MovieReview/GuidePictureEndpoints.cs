using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;
using MyNotes.Security;

namespace MyNotes.Modules.MovieReview;

public static class GuidePictureEndpoints
{
    public static void MapGuidePictures(this WebApplication app)
    {
        app.MapGet("/moviereview/guide/pictures/{id:int}", async Task<IResult>
            (int id, HttpContext context, PrivateAccessStore access, DbContextOptions<AppDbContext> options) =>
        {
            if (!access.IsUnlocked(context.User)) return Results.Unauthorized();
            await using var db = new AppDbContext(options);
            var picture = await db.GuidePictures.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            if (picture is null) return Results.NotFound();
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            return Results.File(picture.Content, picture.ContentType);
        }).RequireAuthorization("PrivateArea");
    }
}
