using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Timetable.Api.Services;

namespace Timetable.Api.Controllers;

[Authorize]
[ApiController]
public abstract class AuthorizedController : ControllerBase
{
    protected int CallerId => CurrentUser.Id(User);
}
