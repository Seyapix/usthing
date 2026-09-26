using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using Timetable.Api.Domain;

namespace Timetable.Api.Services;

public class RecurrenceService
{
    public const int MaxWindowDays = 366;

    public void EnsureWindow(DateTime from, DateTime to)
    {
        if (to <= from)
            throw new ArgumentException("to must be after from.", nameof(to));

        if (to - from > TimeSpan.FromDays(MaxWindowDays))
            throw new ArgumentException($"The range cannot be longer than {MaxWindowDays} days.", nameof(to));
    }

    public string NormalizeTimeZoneId(string? timeZoneId)
    {
        var trimmed = timeZoneId?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return TimetableEvent.DefaultTimeZoneId;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(trimmed).Id;
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ArgumentException("Unknown time zone.", nameof(timeZoneId));
        }
    }

    public string? NormalizeRule(string? rule)
    {
        var trimmed = rule?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        _ = new RecurrencePattern(trimmed);
        return trimmed;
    }

    // One VEVENT per series. Calendar apps expand the rule; this file does not.
    public string ToCalendar(IEnumerable<TimetableEvent> series)
    {
        var calendar = new Calendar();
        foreach (var timetableEvent in series)
            calendar.Events.Add(CreateCalendarEvent(timetableEvent));

        return new CalendarSerializer().SerializeToString(calendar) ?? string.Empty;
    }

    public IReadOnlyList<ImportedEvent> ReadEvents(string calendarText)
    {
        if (string.IsNullOrWhiteSpace(calendarText))
            throw new ArgumentException("The calendar has no events.");

        Calendar calendar;
        try
        {
            calendar = Calendar.Load(calendarText) ?? throw new ArgumentException("The calendar could not be read.");
        }
        catch (Exception exception) when (exception is not ArgumentException)
        {
            throw new ArgumentException("The calendar could not be read.");
        }

        var events = new List<ImportedEvent>();
        foreach (var calendarEvent in calendar.Events)
        {
            if (TryRead(calendarEvent) is { } imported)
                events.Add(imported);
        }

        return events;
    }

    public IReadOnlyList<(DateTime Start, DateTime End)> Occurrences(TimetableEvent timetableEvent, DateTime from, DateTime to)
    {
        if (timetableEvent.RecurrenceRule is null)
        {
            if (timetableEvent.EndUtc > from && timetableEvent.StartUtc < to)
                return [(Utc(timetableEvent.StartUtc), Utc(timetableEvent.EndUtc))];

            return [];
        }

        var calendarEvent = CreateCalendarEvent(timetableEvent);
        // Occurrences are evaluated only after the event belongs to a calendar.
        new Calendar().Events.Add(calendarEvent);

        return calendarEvent.GetOccurrences(UtcCal(from))
            .TakeWhileBefore(UtcCal(to))
            .Select(occurrence =>
            {
                var period = occurrence.Period;
                var end = period.EffectiveEndTime ?? period.StartTime;
                return (Start: Utc(period.StartTime.AsUtc), End: Utc(end.AsUtc));
            })
            .Where(occurrence => occurrence.End > from && occurrence.Start < to)
            .ToList();
    }

    // Duration is applied in the event's time zone, so a Monday 10:00 stays on Monday.
    private static CalendarEvent CreateCalendarEvent(TimetableEvent timetableEvent)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timetableEvent.TimeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(Utc(timetableEvent.StartUtc), zone);
        var start = new CalDateTime(local.Year, local.Month, local.Day, local.Hour, local.Minute, local.Second, timetableEvent.TimeZoneId);
        var duration = timetableEvent.EndUtc - timetableEvent.StartUtc;

        var calendarEvent = new CalendarEvent
        {
            Uid = timetableEvent.ExternalUid ?? timetableEvent.Id.ToString(),
            Summary = timetableEvent.Title,
            Location = timetableEvent.Location,
            Description = timetableEvent.Description,
            DtStart = start,
            DtEnd = start.Add(new Duration(
                days: duration.Days,
                hours: duration.Hours,
                minutes: duration.Minutes,
                seconds: duration.Seconds)),
        };

        if (timetableEvent.RecurrenceRule is not null)
            calendarEvent.RecurrenceRules.Add(new RecurrencePattern(timetableEvent.RecurrenceRule));

        return calendarEvent;
    }

    private static CalDateTime UtcCal(DateTime value)
    {
        var utc = Utc(value);
        return new CalDateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, "UTC");
    }

    private ImportedEvent? TryRead(CalendarEvent calendarEvent)
    {
        var start = calendarEvent.DtStart;
        if (start is null)
            return null;

        DateTime endUtc;
        if (calendarEvent.DtEnd is { } end)
            endUtc = Utc(end.AsUtc);
        else if (calendarEvent.Duration is { } duration)
            endUtc = Utc(start.AsUtc).Add(duration.ToTimeSpan(start));
        else
            return null;

        var rule = calendarEvent.RecurrenceRules.FirstOrDefault()?.ToString();
        if (rule is not null && rule.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            rule = rule["RRULE:".Length..];

        var uid = calendarEvent.Uid?.Trim();
        return new ImportedEvent(
            string.IsNullOrEmpty(uid) ? null : uid,
            calendarEvent.Summary ?? "",
            calendarEvent.Location,
            calendarEvent.Description,
            Utc(start.AsUtc),
            endUtc,
            NormalizeRule(rule),
            NormalizeTimeZoneId(start.TzId));
    }

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public sealed record ImportedEvent(
        string? Uid,
        string Title,
        string? Location,
        string? Description,
        DateTime StartUtc,
        DateTime EndUtc,
        string? RecurrenceRule,
        string TimeZoneId);
}
