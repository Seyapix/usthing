using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

using Timetable.Api.Exceptions;

namespace Timetable.Api.Middleware;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger, IProblemDetailsService problemDetails) : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger = logger;
    private readonly IProblemDetailsService _problemDetails = problemDetails;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var status = StatusCodes.Status500InternalServerError;
        var detail = "An unexpected error occurred.";

        switch (exception)
        {
            case ApiException apiException:
                status = apiException.StatusCode;
                detail = apiException.Message;
                break;
            case ArgumentException argumentException:
                status = StatusCodes.Status400BadRequest;
                detail = argumentException.Message;
                break;
            default:
                _logger.LogError(exception, "Unhandled exception");
                break;
        }

        httpContext.Response.StatusCode = status;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = ReasonPhrases.GetReasonPhrase(status),
                Detail = detail,
            },
        });
    }
}
