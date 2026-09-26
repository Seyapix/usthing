namespace Timetable.Api.Settings;

public class JwtSettings
{
    public string Issuer { get; init; } = "";

    public string Audience { get; init; } = "";

    public string SigningKey { get; init; } = "";

    public int ExpiresMinutes { get; init; }

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(Issuer)
            || string.IsNullOrWhiteSpace(Audience)
            || SigningKey.Length < 32
            || ExpiresMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt settings are incomplete.");
        }
    }
}
