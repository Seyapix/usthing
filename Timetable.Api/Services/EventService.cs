using Microsoft.EntityFrameworkCore;

using Timetable.Api.Contracts;
using Timetable.Api.Data;
using Timetable.Api.Domain;
using Timetable.Api.Exceptions;

namespace Timetable.Api.Services;

public class EventService(TimetableContext context)
{
    private readonly TimetableContext _context = context;

    public async Task<List<EventResponse>> List(int callerId, DateTime? from, DateTime? to)
    {
        var query = OwnedBy(callerId).AsNoTracking();
        if (from is DateTime fromUtc)
            query = query.Where(x => x.EndUtc > fromUtc);
        if (to is DateTime toUtc)
            query = query.Where(x => x.StartUtc < toUtc);

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
            request.Start,
            request.End);

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
            request.Start,
            request.End);

        await _context.SaveChangesAsync();
        return ToResponse(timetableEvent);
    }

    public async Task Delete(int callerId, int id)
    {
        var timetableEvent = await FindOwned(callerId, id);
        _context.TimetableEvents.Remove(timetableEvent);
        await _context.SaveChangesAsync();
    }

    private async Task<TimetableEvent> FindOwned(int callerId, int id)
    {
        var timetableEvent = await OwnedBy(callerId).FirstOrDefaultAsync(x => x.Id == id);
        if (timetableEvent is null)
            throw new ApiException(StatusCodes.Status404NotFound, "Event not found.");

        return timetableEvent;
    }

    // Another user's row is not in this query, so a foreign id is a 404.
    private IQueryable<TimetableEvent> OwnedBy(int callerId) =>
        _context.TimetableEvents.Where(x => x.UserId == callerId);

    private static string RequiredTitle(string title)
    {
        var trimmed = title.Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("Title is required.", nameof(title));

        return trimmed;
    }

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static EventResponse ToResponse(TimetableEvent timetableEvent) => new()
    {
        Id = timetableEvent.Id,
        Title = timetableEvent.Title,
        Location = timetableEvent.Location,
        Description = timetableEvent.Description,
        Start = Utc(timetableEvent.StartUtc),
        End = Utc(timetableEvent.EndUtc),
        CreatedAt = Utc(timetableEvent.CreatedAt),
        UpdatedAt = Utc(timetableEvent.UpdatedAt),
    };
}
