using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Timetable.Api.Exceptions;

namespace Timetable.Api.Controllers;

[Authorize]
[ApiController]
public abstract class AuthorizedController : ControllerBase
{
    protected int CallerId
    {
        get
        {
            if (!int.TryParse(User.Identity?.Name, out var userId))
                throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid token.");

            return userId;
        }
    }
}
