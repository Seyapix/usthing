using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Timetable.Api.Contracts;
using Timetable.Api.Services;

namespace Timetable.Api.Controllers.V1;

[ApiController]
[Route("v1/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    /// <summary>Issue a bearer token.</summary>
    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<TokenResponse> Token(LoginRequest request) =>
        await _authService.SignIn(request.Username, request.Password);
}
