using Microsoft.AspNetCore.Mvc;

using Timetable.Api.Contracts;
using Timetable.Api.Services;

namespace Timetable.Api.Controllers.V1;

[Route("v1/events")]
public class EventsController(EventService eventService) : AuthorizedController
{
    private readonly EventService _eventService = eventService;

    /// <summary>List the caller's events. Optional from and to limit by overlap.</summary>
    [HttpGet]
    public async Task<List<EventResponse>> List([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        await _eventService.List(CallerId, from, to);

    /// <summary>Get one of the caller's events.</summary>
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
