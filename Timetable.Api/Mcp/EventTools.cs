using System.ComponentModel;
using System.Security.Claims;

using ModelContextProtocol.Server;

using Timetable.Api.Contracts;
using Timetable.Api.Services;

namespace Timetable.Api.Mcp;

[McpServerToolType]
public class EventTools
{
    [McpServerTool(Name = "list_events"), Description("List the caller's events. Both from and to expand recurrences.")]
    public Task<List<EventResponse>> ListEvents(
        EventService eventService,
        ClaimsPrincipal user,
        DateTime? from = null,
        DateTime? to = null) =>
        eventService.List(CurrentUser.Id(user), from, to);

    [McpServerTool(Name = "create_event"), Description("Create an event for the caller.")]
    public Task<EventResponse> CreateEvent(
        EventService eventService,
        ClaimsPrincipal user,
        string title,
        DateTime start,
        DateTime end,
        string? location = null,
        string? description = null,
        string? recurrenceRule = null,
        string? timeZoneId = null) =>
        eventService.Create(CurrentUser.Id(user), new EventWriteRequest
        {
            Title = title,
            Location = location,
            Description = description,
            Start = start,
            End = end,
            RecurrenceRule = recurrenceRule,
            TimeZoneId = timeZoneId,
        });

    [McpServerTool(Name = "update_event"), Description("Replace one of the caller's events.")]
    public Task<EventResponse> UpdateEvent(
        EventService eventService,
        ClaimsPrincipal user,
        int id,
        string title,
        DateTime start,
        DateTime end,
        string? location = null,
        string? description = null,
        string? recurrenceRule = null,
        string? timeZoneId = null) =>
        eventService.Update(CurrentUser.Id(user), id, new EventWriteRequest
        {
            Title = title,
            Location = location,
            Description = description,
            Start = start,
            End = end,
            RecurrenceRule = recurrenceRule,
            TimeZoneId = timeZoneId,
        });

    [McpServerTool(Name = "delete_event"), Description("Delete one of the caller's events.")]
    public async Task<int> DeleteEvent(EventService eventService, ClaimsPrincipal user, int id)
    {
        await eventService.Delete(CurrentUser.Id(user), id);
        return id;
    }
}
