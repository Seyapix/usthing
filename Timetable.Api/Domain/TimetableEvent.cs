namespace Timetable.Api.Domain;

public class TimetableEvent
{
    public const string DefaultTimeZoneId = "Asia/Hong_Kong";

    public int Id { get; private set; }

    public int UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public string? Description { get; private set; }

    public DateTime StartUtc { get; private set; }

    public DateTime EndUtc { get; private set; }

    public string? RecurrenceRule { get; private set; }

    public string TimeZoneId { get; private set; } = DefaultTimeZoneId;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; } = null!;

    public static TimetableEvent Create(
        int userId,
        string title,
        string? location,
        string? description,
        DateTime startUtc,
        DateTime endUtc,
        string? recurrenceRule,
        string timeZoneId)
    {
        EnsureEndAfterStart(startUtc, endUtc);

        var now = DateTime.UtcNow;
        return new TimetableEvent
        {
            UserId = userId,
            Title = title,
            Location = location,
            Description = description,
            StartUtc = startUtc,
            EndUtc = endUtc,
            RecurrenceRule = recurrenceRule,
            TimeZoneId = timeZoneId,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(
        string title,
        string? location,
        string? description,
        DateTime startUtc,
        DateTime endUtc,
        string? recurrenceRule,
        string timeZoneId)
    {
        EnsureEndAfterStart(startUtc, endUtc);

        Title = title;
        Location = location;
        Description = description;
        StartUtc = startUtc;
        EndUtc = endUtc;
        RecurrenceRule = recurrenceRule;
        TimeZoneId = timeZoneId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void EnsureEndAfterStart(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
            throw new ArgumentException("End must be after start.", nameof(endUtc));
    }
}
