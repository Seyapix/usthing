namespace Timetable.Api.Domain;

public class User
{
    public int Id { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public static User Create(string username) => new()
    {
        Username = username,
    };
}
