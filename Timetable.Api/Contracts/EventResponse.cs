namespace Timetable.Api.Contracts;

public class EventResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = "";

    public string? Location { get; init; }

    public string? Description { get; init; }

    public DateTime Start { get; init; }

    public DateTime End { get; init; }

    public string? RecurrenceRule { get; init; }

    public string TimeZoneId { get; init; } = "";

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
