namespace Timetable.Api.Domain;

public class User
{
    public int Id { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public static User Create(string username, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Username = username,
            PasswordHash = passwordHash,
        };
    }
}
