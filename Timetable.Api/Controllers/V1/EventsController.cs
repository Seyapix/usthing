using Microsoft.AspNetCore.Mvc;

using Timetable.Api.Contracts;
using Timetable.Api.Services;

namespace Timetable.Api.Controllers.V1;

[Route("v1/events")]
public class EventsController(EventService eventService) : AuthorizedController
{
    private readonly EventService _eventService = eventService;

    /// <summary>List the caller's events. A from and to range expands recurrences; one bound does not.</summary>
    [HttpGet]
    public async Task<List<EventResponse>> List([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        await _eventService.List(CallerId, from, to);

    /// <summary>Export the caller's events in the range as an iCalendar file.</summary>
    [HttpGet("export.ics")]
    public async Task<IActionResult> Export([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (from is null || to is null)
            throw new ArgumentException("from and to are required.");

        var calendar = await _eventService.Export(CallerId, from.Value, to.Value);
        return Content(calendar, "text/calendar; charset=utf-8");
    }

    /// <summary>Get one of the caller's events. A recurring event is returned as the series.</summary>
    [HttpGet("{id:int}")]
    public async Task<EventResponse> Get(int id) =>
        await _eventService.Get(CallerId, id);

    /// <summary>Create an event for the caller.</summary>
    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(EventWriteRequest request)
    {
        var created = await _eventService.Create(CallerId, request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Replace one of the caller's events.</summary>
    [HttpPut("{id:int}")]
    public async Task<EventResponse> Update(int id, EventWriteRequest request) =>
        await _eventService.Update(CallerId, id, request);

    /// <summary>Delete one of the caller's events.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _eventService.Delete(CallerId, id);
        return NoContent();
    }
}
