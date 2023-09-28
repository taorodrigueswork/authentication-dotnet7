using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace API.CustomMiddlewares;

/// <summary>
/// For more information: https://code-maze.com/global-error-handling-aspnetcore/
/// </summary>
[ExcludeFromCodeCoverage]
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _requestDelegate;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate requestDelegate, 
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _requestDelegate = requestDelegate;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _requestDelegate(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(0, ex, $"[ErrorHandlingMiddleware] | [ErrorMessage]: {ex.Message} ");

            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        switch (exception)
        {
            case ApplicationException ex:
                if (ex.Message.Contains("Invalid Token") ||
                ex.Message.Contains("This account is blocked/locked out") ||
                ex.Message.Contains("This account doesn't have permission to login in this system.") ||
                ex.Message.Contains("You need to confirm login in your 2FA") ||
                ex.Message.Contains("User was not created") ||
                    ex.Message.Contains("Username or password is wrong"))
                {
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                }
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case ArgumentNullException ex:
                if (ex.Message.Contains("not found"))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                }
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var result = JsonSerializer.Serialize(exception.Message);
        await context.Response.WriteAsync(result);
    }
}