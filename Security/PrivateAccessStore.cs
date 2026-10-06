using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace MyNotes.Security;

public sealed class PrivateAccessStore(IConfiguration configuration)
{
    public const string SessionClaim = "private-session";
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);
    private readonly ConcurrentDictionary<string, DateTimeOffset> sessions = new();
    private readonly string passwordFile = configuration["PrivateAccess:PasswordFile"]
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyNotes", "private-access.json");

    public bool IsConfigured => File.Exists(passwordFile);

    public bool VerifyPassword(string password)
    {
        if (!IsConfigured || password.Length > 256) return false;
        var record = JsonSerializer.Deserialize<PasswordRecord>(File.ReadAllText(passwordFile));
        if (record is null) return false;
        var salt = Convert.FromBase64String(record.Salt);
        var expected = Convert.FromBase64String(record.Hash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, record.Iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    public string CreateSession()
    {
        foreach (var session in sessions.Where(x => x.Value <= DateTimeOffset.UtcNow))
            sessions.TryRemove(session.Key, out _);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        sessions[token] = DateTimeOffset.UtcNow.Add(SessionLifetime);
        return token;
    }

    public bool IsUnlocked(ClaimsPrincipal user)
    {
        var token = user.FindFirstValue(SessionClaim);
        return user.Identity?.IsAuthenticated == true && token is not null
            && sessions.TryGetValue(token, out var expiry) && expiry > DateTimeOffset.UtcNow;
    }

    public void Revoke(ClaimsPrincipal user)
    {
        if (user.FindFirstValue(SessionClaim) is { } token) sessions.TryRemove(token, out _);
    }

    private sealed record PasswordRecord(string Salt, string Hash, int Iterations);
}
