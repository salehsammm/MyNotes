using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MovieReviews.Data;

namespace MyNotes.Security;

// Gate every MovieReview database operation, including actions from an existing Blazor circuit.
public sealed class PrivateMovieDbContextFactory(DbContextOptions<AppDbContext> options,
    AuthenticationStateProvider authenticationState, PrivateAccessStore access) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        EnsureAccessAsync().GetAwaiter().GetResult();
        return new AppDbContext(options);
    }

    public async Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await EnsureAccessAsync();
        return new AppDbContext(options);
    }

    private async Task EnsureAccessAsync()
    {
        var state = await authenticationState.GetAuthenticationStateAsync();
        if (!access.IsUnlocked(state.User)) throw new UnauthorizedAccessException("Unlock the private area to continue.");
    }
}
