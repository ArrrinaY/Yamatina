using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using InternshipService.Models.DTO;
using Npgsql;

namespace InternshipService.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title) = MapExceptionToStatusCode(exception);
        context.Response.StatusCode = statusCode;
        
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Internal Server Error: {Message}\nStack Trace: {StackTrace}", 
                exception.Message, exception.StackTrace);
        }
        else
        {
            logger.LogInformation("{code} error: {title}, more: {message}", statusCode, title, exception.Message);
        }
        
        var response = new ExceptionModel()
        {
            Error = title,
            Message = GetExceptionDetail(exception)
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    
    private static (int StatusCode, string Title) MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
            ArgumentException or ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid Operation"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Not logged into account"),
            SecurityTokenException => (StatusCodes.Status403Forbidden, "Not had enough permissions"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            OperationCanceledException => (StatusCodes.Status408RequestTimeout, "Request Timeout"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict"),
            Npgsql.PostgresException pgEx => pgEx.SqlState switch
            {
                "23503" => (StatusCodes.Status400BadRequest, "Referenced entity does not exist or cannot be used."),
                "23505" => (StatusCodes.Status409Conflict, "Entity with this data already exists."),
                _ => (StatusCodes.Status500InternalServerError, "Database Error")
            },
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }
    
    private static string GetExceptionDetail(Exception exception)
    {
        return exception switch
        {
            ValidationException => exception.Message,
            ArgumentException => exception.Message,
            InvalidOperationException => exception.Message,
            UnauthorizedAccessException => exception.Message,
            SecurityTokenException => exception.Message,
            KeyNotFoundException => exception.Message,
            DbUpdateConcurrencyException => exception.Message,
            Npgsql.PostgresException pgEx => pgEx.Detail ?? pgEx.Message,
            _ => "An error occurred while processing your request"
        };
    }
}

