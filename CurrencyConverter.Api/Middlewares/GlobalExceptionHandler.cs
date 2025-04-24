using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerManager _logger;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILoggerManager logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string errorMessage;
        int statusCode;
        string errorCode;

        switch (exception)
        {
            case CustomException customEx:
                statusCode = customEx.StatusCode;
                errorMessage = customEx.Message;
                errorCode = customEx.ErrorCode;
                _logger.LogError($"Custom exception: {errorCode} - {errorMessage}");
                break;

            case HttpRequestException httpEx when httpEx.Message.Contains("Rate limit"):
                statusCode = StatusCodes.Status429TooManyRequests;
                errorMessage = "Rate limit exceeded for external service.";
                errorCode = "RATE_LIMIT_EXCEEDED";
                _logger.LogError($"Rate limit error: {errorMessage}");
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                errorMessage = "An unexpected error occurred.";
                errorCode = "INTERNAL_SERVER_ERROR";
                _logger.LogError($"Unexpected error: {exception.Message}\n{exception.StackTrace}");
                break;
        }
        
        if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests && exception == null)
        {
            statusCode = StatusCodes.Status429TooManyRequests;
            errorMessage = "Too many requests. Please try again later.";
            errorCode = "TOO_MANY_REQUESTS";
            _logger.LogWarn($"IP rate limit exceeded for {context.Connection.RemoteIpAddress}");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResponse = new
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = errorMessage,
            Timestamp = DateTime.UtcNow
        };

        string json = JsonSerializer.Serialize(errorResponse, JsonSerializerOptions);
        await context.Response.WriteAsync(json);
    }
}

// Extension method to register the middleware
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
