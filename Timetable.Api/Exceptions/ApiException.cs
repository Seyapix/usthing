namespace Timetable.Api.Exceptions;

public class ApiException(int statusCode, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
}
