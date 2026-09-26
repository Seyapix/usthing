using Microsoft.EntityFrameworkCore;

using Timetable.Api.Contracts;
using Timetable.Api.Data;
using Timetable.Api.Domain;
using Timetable.Api.Exceptions;

namespace Timetable.Api.Services;

public class EventService(TimetableContext context, RecurrenceService recurrence)
{
    private readonly TimetableContext _context = context;
    private readonly RecurrenceService _recurrence = recurrence;

    public async Task<List<EventResponse>> List(int callerId, DateTime? from, DateTime? to)
    {
        var fromUtc = from is DateTime windowFrom ? ToUtc(windowFrom) : (DateTime?)null;
        var toUtc = to is DateTime windowTo ? ToUtc(windowTo) : (DateTime?)null;
        if (fromUtc is DateTime rangeFrom && toUtc is DateTime rangeTo)
            return await ListOccurrences(callerId, rangeFrom, rangeTo);

        var query = OwnedBy(callerId).AsNoTracking();
        if (fromUtc is DateTime fromOnly)
            query = query.Where(x => x.EndUtc > fromOnly);
        if (toUtc is DateTime toOnly)
            query = query.Where(x => x.StartUtc < toOnly);

        var events = await query.OrderBy(x => x.StartUtc).ToListAsync();
        return events.Select(ToResponse).ToList();
    }

    public async Task<EventResponse> Get(int callerId, int id)
    {
        var timetableEvent = await OwnedBy(callerId).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (timetableEvent is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Event not found.");

        return ToResponse(timetableEvent);
    }

    public async Task<EventResponse> Create(int callerId, EventWriteRequest request)
    {
        var timetableEvent = TimetableEvent.Create(
            callerId,
            RequiredTitle(request.Title),
            Clean(request.Location),
            Clean(request.Description),
            ToUtc(request.Start),
            ToUtc(request.End),
            _recurrence.NormalizeRule(request.RecurrenceRule),
            _recurrence.NormalizeTimeZoneId(request.TimeZoneId));

        _context.TimetableEvents.Add(timetableEvent);
        await _context.SaveChangesAsync();
        return ToResponse(timetableEvent);
    }

    public async Task<EventResponse> Update(int callerId, int id, EventWriteRequest request)
    {
        var timetableEvent = await FindOwned(callerId, id);
        timetableEvent.Update(
            RequiredTitle(request.Title),
            Clean(request.Location),
            Clean(request.Description),
            ToUtc(request.Start),
            ToUtc(request.End),
            _recurrence.NormalizeRule(request.RecurrenceRule),
            _recurrence.NormalizeTimeZoneId(request.TimeZoneId));

        await _context.SaveChangesAsync();
        return ToResponse(timetableEvent);
    }

    public async Task<List<EventResponse>> Import(int callerId, string calendarText)
    {
        var imported = _recurrence.ReadEvents(calendarText);
        if (imported.Count == 0)
            throw new ArgumentException("The calendar has no events.");

        var owned = await OwnedBy(callerId).ToListAsync();
        var saved = new List<TimetableEvent>();
        foreach (var row in imported)
        {
            var match = MatchImport(owned, row.Uid);
            if (match is null)
            {
                match = TimetableEvent.Create(
                    callerId,
                    RequiredTitle(row.Title),
                    Clean(row.Location),
                    Clean(row.Description),
                    row.StartUtc,
                    row.EndUtc,
                    row.RecurrenceRule,
                    row.TimeZoneId,
                    row.Uid);
                _context.TimetableEvents.Add(match);
                owned.Add(match);
            }
            else
            {
                match.Update(
                    RequiredTitle(row.Title),
                    Clean(row.Location),
                    Clean(row.Description),
                    row.StartUtc,
                    row.EndUtc,
                    row.RecurrenceRule,
                    row.TimeZoneId);
                if (match.ExternalUid is null && row.Uid is not null)
                    match.AssignExternalUid(row.Uid);
            }

            saved.Add(match);
        }

        await _context.SaveChangesAsync();
        return saved.Select(ToResponse).ToList();
    }

    public async Task<string> Export(int callerId, DateTime from, DateTime to)
    {
        from = ToUtc(from);
        to = ToUtc(to);
        _recurrence.EnsureWindow(from, to);
        var events = await InWindow(callerId, from, to).ToListAsync();
        var series = events.Where(timetableEvent => _recurrence.Occurrences(timetableEvent, from, to).Count > 0);
        return _recurrence.ToCalendar(series);
    }

    public async Task Delete(int callerId, int id)
    {
        var timetableEvent = await FindOwned(callerId, id);
        _context.TimetableEvents.Remove(timetableEvent);
        await _context.SaveChangesAsync();
    }

    private async Task<List<EventResponse>> ListOccurrences(int callerId, DateTime from, DateTime to)
    {
        _recurrence.EnsureWindow(from, to);
        var events = await InWindow(callerId, from, to).ToListAsync();
        return events
            .SelectMany(timetableEvent => _recurrence.Occurrences(timetableEvent, from, to)
                .Select(occurrence => ToResponse(timetableEvent, occurrence.Start, occurrence.End)))
            .OrderBy(response => response.Start)
            .ThenBy(response => response.Id)
            .ToList();
    }

    private async Task<TimetableEvent> FindOwned(int callerId, int id)
    {
        var timetableEvent = await OwnedBy(callerId).FirstOrDefaultAsync(x => x.Id == id);
        if (timetableEvent is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Event not found.");

        return timetableEvent;
    }

    // Our own export uses {id}@timetable until a uid is stored. Any other uid is a new event.
    private static TimetableEvent? MatchImport(List<TimetableEvent> owned, string? uid)
    {
        if (string.IsNullOrEmpty(uid))
            return null;

        var byUid = owned.FirstOrDefault(x => x.ExternalUid == uid);
        if (byUid is not null)
            return byUid;

        if (!RecurrenceService.TryParseExportedId(uid, out var id))
            return null;

        return owned.FirstOrDefault(x => x.Id == id && x.ExternalUid is null);
    }

    // Another user's row is not in this query, so a foreign id is a 404.
    private IQueryable<TimetableEvent> OwnedBy(int callerId) =>
        _context.TimetableEvents.Where(x => x.UserId == callerId);

    // The first instance can sit outside the window, so a recurring row is not filtered by its own end.
    private IQueryable<TimetableEvent> InWindow(int callerId, DateTime from, DateTime to) =>
        OwnedBy(callerId)
            .AsNoTracking()
            .Where(x => x.StartUtc < to && (x.RecurrenceRule != null || x.EndUtc > from))
            .OrderBy(x => x.StartUtc);

    private static string RequiredTitle(string title)
    {
        var trimmed = title.Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("Title is required.", nameof(title));

        return trimmed;
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static EventResponse ToResponse(TimetableEvent timetableEvent) =>
        ToResponse(timetableEvent, timetableEvent.StartUtc, timetableEvent.EndUtc);

    private static EventResponse ToResponse(TimetableEvent timetableEvent, DateTime start, DateTime end) => new()
    {
        Id = timetableEvent.Id,
        Title = timetableEvent.Title,
        Location = timetableEvent.Location,
        Description = timetableEvent.Description,
        Start = ToUtc(start),
        End = ToUtc(end),
        RecurrenceRule = timetableEvent.RecurrenceRule,
        TimeZoneId = timetableEvent.TimeZoneId,
        CreatedAt = ToUtc(timetableEvent.CreatedAt),
        UpdatedAt = ToUtc(timetableEvent.UpdatedAt),
    };
}
