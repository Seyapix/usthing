using System.ComponentModel.DataAnnotations;

namespace Timetable.Api.Contracts;

public class EventWriteRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Location { get; set; }

    public string? Description { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? RecurrenceRule { get; set; }

    public string? TimeZoneId { get; set; }
}
