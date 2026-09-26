using System.Security.Claims;

using Timetable.Api.Exceptions;

namespace Timetable.Api.Services;

public static class CurrentUser
{
    public static int Id(ClaimsPrincipal user)
    {
        if (!int.TryParse(user.Identity?.Name, out var userId))
            throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid token.");

        return userId;
    }
}
