using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;
using MovieReviews.Services;

namespace MyNotes.Modules.MovieReview;

public static class MovieReviewModule
{
    public static IServiceCollection AddMovieReviewModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MovieReview")));
        services.AddScoped<PerformerService>();
        services.AddScoped<LookupService>();
        services.AddScoped<SceneService>();
        services.AddScoped<StudioService>();
        services.AddScoped<GuideService>();
        services.AddScoped<IdeaService>();
        services.AddScoped<NoteService>();
        services.AddScoped<ResearchService>();
        services.AddScoped<TrophyService>();
        return services;
    }
}
