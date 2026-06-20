using AgileFlow.Domain.Entities;

namespace AgileFlow.Domain.Entities;

/// <summary>
/// Persisted refresh token. Rotated on every use — old token is revoked, new one issued.
/// </summary>
public class RefreshToken
{
    public int Id { get; private set; }

    /// <summary>Cryptographically random 64-byte Base64 string.</summary>
    public string Token { get; private set; } = string.Empty;

    public string UserId { get; private set; } = string.Empty;
    public AppUser AppUser { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; set; }          // setter needed by AuthService

    private RefreshToken() { }

    public RefreshToken(string token, string userId, DateTime expiresAt)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public void Revoke() => IsRevoked = true;
}