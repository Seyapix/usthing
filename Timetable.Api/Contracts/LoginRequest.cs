using System.ComponentModel.DataAnnotations;

namespace Timetable.Api.Contracts;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}
