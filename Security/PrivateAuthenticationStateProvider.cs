using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace MyNotes.Security;

public sealed class PrivateAuthenticationStateProvider(ILoggerFactory loggerFactory, PrivateAccessStore access)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(10);

    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
        => Task.FromResult(access.IsUnlocked(authenticationState.User));
}
