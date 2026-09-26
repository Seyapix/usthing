using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Timetable.Api.Data;
using Timetable.Api.Domain;

namespace Timetable.Api.Services;

public class DevelopmentUserSeeder(TimetableContext context, PasswordHasher<User> passwordHasher)
{
    private readonly TimetableContext _context = context;
    private readonly PasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task Seed()
    {
        if (await _context.Users.AnyAsync())
            return;

        _context.Users.Add(Create("alice", "alice"));
        _context.Users.Add(Create("bob", "bob"));
        await _context.SaveChangesAsync();
    }

    private User Create(string username, string password) =>
        User.Create(username, _passwordHasher.HashPassword(null!, password));
}
