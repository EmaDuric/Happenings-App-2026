using System.Net;
using Happenings.Model.Exceptions;

namespace Happenings.WebAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        // Mapiranje po TIPU exceptiona (ne po tekstu poruke).
        catch (NotFoundException ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, ex.Message, ex, "Resource not found");
        }
        catch (ForbiddenException ex)
        {
            await WriteError(context, HttpStatusCode.Forbidden, ex.Message, ex, "Forbidden action");
        }
        catch (ConflictException ex)
        {
            await WriteError(context, HttpStatusCode.Conflict, ex.Message, ex, "Conflict");
        }
        catch (BusinessRuleException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message, ex, "Business rule violation");
        }
        catch (UnauthorizedException ex)
        {
            await WriteError(context, HttpStatusCode.Unauthorized, ex.Message, ex, "Authentication failed");
        }
        // Framework tipovi � zadrzani radi sigurnosti (mogu doci iz biblioteka/EF).
        catch (UnauthorizedAccessException ex)
        {
            await WriteError(context, HttpStatusCode.Forbidden,
                "You do not have permission to perform this action", ex, "Forbidden access attempt");
        }
        catch (KeyNotFoundException ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, ex.Message, ex, "Resource not found");
        }
        catch (ArgumentException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message, ex, "Invalid argument");
        }
        catch (Exception ex)
        {
            // Genericka greska � ne otkriva interne detalje
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                status = 500,
                message = "An unexpected error occurred. Please try again later."
            });
        }
    }

    private async Task WriteError(HttpContext context, HttpStatusCode status, string message,
        Exception ex, string logMessage)
    {
        _logger.LogWarning(ex, logMessage);
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(new
        {
            status = (int)status,
            message
        });
    }
}