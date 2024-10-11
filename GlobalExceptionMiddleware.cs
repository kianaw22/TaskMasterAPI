using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext); // Proceed to the next middleware or controller action
        }
        catch (Exception ex) // Catch any unhandled exception
        {
            _logger.LogError($"An error occurred: {ex.Message}"); // Log the exception
            await HandleExceptionAsync(httpContext, ex); // Handle the exception and return a response
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // Default to 500 Internal Server Error

        if (exception is UnauthorizedAccessException)
        {
            statusCode = HttpStatusCode.Forbidden; // 403 Forbidden
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = HttpStatusCode.NotFound; // 404 Not Found
        }
        else if (exception is InvalidOperationException)
        {
            statusCode = HttpStatusCode.BadRequest; // 400 Bad Request
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode; // Convert to integer for the HTTP status code

        return context.Response.WriteAsync(new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message // Custom error message from the exception
        }.ToString());
    }
}

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this); // Return JSON formatted error
    }
}
