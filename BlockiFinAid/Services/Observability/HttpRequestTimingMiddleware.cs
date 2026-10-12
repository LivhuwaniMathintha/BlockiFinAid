using System.Diagnostics;

namespace BlockiFinAid.Services.Observability;

public sealed class HttpRequestTimingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, OperationTimingTracker timingTracker)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var route = context.GetEndpoint()?.Metadata.GetMetadata<RouteEndpoint>()?.RoutePattern.RawText
                        ?? "unmatched";
            timingTracker.RecordHttpRequest(
                context.Request.Method,
                route,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
