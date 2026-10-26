using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent API response while keeping
/// exception details in server-side logs only.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
            _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.Response.StatusCode,
                System.Diagnostics.Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, context.TraceIdentifier);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request was cancelled by the client. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted) throw;

            var statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                DbUpdateException or InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            var expected = statusCode < 500;
            if (expected)
                _logger.LogWarning(exception, "Request rejected for {Method} {Path}. StatusCode: {StatusCode}; TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, statusCode, context.TraceIdentifier);
            else
                _logger.LogError(exception, "Unhandled exception processing {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch { 400 => "Invalid request.", 409 => "The request conflicts with existing data.", _ => "An unexpected error occurred." },
                Detail = statusCode switch { 400 => exception.Message, 409 => "The operation conflicts with existing data or related records.", _ => "The request could not be completed. Use the trace ID when contacting support." }
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
