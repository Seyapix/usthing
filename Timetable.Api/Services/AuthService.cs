using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Timetable.Api.Contracts;
using Timetable.Api.Data;
using Timetable.Api.Domain;
using Timetable.Api.Exceptions;
using Timetable.Api.Settings;

namespace Timetable.Api.Services;

public class AuthService(TimetableContext context, PasswordHasher<User> passwordHasher, JwtSettings jwt)
{
    private readonly TimetableContext _context = context;
    private readonly PasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly JwtSettings _jwt = jwt;

    public async Task<TokenResponse> SignIn(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username);
        if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid username or password.");

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())],
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256));

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
        };
    }
}
