using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace BlockiFinAid.Services.Observability;

public sealed record OperationTimingSummary(
    string Pipeline,
    string Model,
    string Step,
    int Count,
    double AverageMs,
    long MinMs,
    long MaxMs,
    long P95Ms,
    long LastMs);

public sealed record HttpRequestTimingSummary(
    string Method,
    string Route,
    int StatusCode,
    long Count,
    int SampleCount,
    double AverageMs,
    long P95Ms);

public sealed class OperationTimingTracker
{
    private readonly ILogger<OperationTimingTracker> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _durationSamples = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _rateSamples = new();
    private readonly ConcurrentDictionary<HttpEndpointKey, ConcurrentQueue<long>> _httpDurationSamples = new();
    private readonly ConcurrentDictionary<HttpEndpointKey, long> _httpRequestCounts = new();
    private readonly string _summaryFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "timing-dashboard.md");
    private readonly string _drilldownFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "timing-drilldown.txt");
    private readonly object _summaryFileLock = new();

    public OperationTimingTracker(ILogger<OperationTimingTracker> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(Path.GetDirectoryName(_summaryFilePath)!);
    }

    public void RecordStep(string pipeline, string model, string step, long durationMs, string? entityId = null, string? status = "ok")
    {
        var key = $"{pipeline}:{model}:{step}";
        var samples = _durationSamples.GetOrAdd(key, _ => new ConcurrentQueue<long>());
        samples.Enqueue(durationMs);

        while (samples.Count > 500)
        {
            samples.TryDequeue(out _);
        }

        var values = samples.ToArray();
        var count = values.Length;
        var avgMs = count == 0 ? 0 : values.Average();
        var minMs = count == 0 ? 0 : values.Min();
        var maxMs = count == 0 ? 0 : values.Max();
        var p95Ms = count == 0 ? 0 : GetPercentile(values, 95);

        _logger.LogInformation(
            "[Timing] pipeline={Pipeline} model={Model} step={Step} entityId={EntityId} status={Status} durationMs={DurationMs} avgMs={AverageMs} minMs={MinMs} maxMs={MaxMs} p95Ms={P95Ms} count={Count}",
            pipeline,
            model,
            step,
            entityId ?? "n/a",
            status,
            durationMs,
            avgMs,
            minMs,
            maxMs,
            p95Ms,
            count);

        LogSummarySnapshot(pipeline, model, step);
        WriteDashboardSummary();
    }

    public void RecordHttpRequest(string method, string route, int statusCode, long durationMs)
    {
        var key = new HttpEndpointKey(method, route, statusCode);
        var samples = _httpDurationSamples.GetOrAdd(key, _ => new ConcurrentQueue<long>());
        samples.Enqueue(durationMs);

        while (samples.Count > 500)
        {
            samples.TryDequeue(out _);
        }

        _httpRequestCounts.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public IReadOnlyList<OperationTimingSummary> GetTimingSummaries()
    {
        return GetSummaryEntries()
            .Select(entry => new OperationTimingSummary(
                entry.pipeline,
                entry.model,
                entry.step,
                entry.count,
                entry.avgMs,
                entry.minMs,
                entry.maxMs,
                entry.p95Ms,
                entry.lastMs))
            .ToList();
    }

    public IReadOnlyList<HttpRequestTimingSummary> GetHttpRequestSummaries()
    {
        return _httpDurationSamples
            .Select(entry =>
            {
                var values = entry.Value.ToArray();
                return new HttpRequestTimingSummary(
                    entry.Key.Method,
                    entry.Key.Route,
                    entry.Key.StatusCode,
                    _httpRequestCounts.GetValueOrDefault(entry.Key),
                    values.Length,
                    values.Length == 0 ? 0 : values.Average(),
                    GetPercentile(values, 95));
            })
            .ToList();
    }

    public void RecordQueueLatency(string model, DateTime enqueuedAtUtc, DateTime processedAtUtc, string? entityId = null)
    {
        var latencyMs = (long)Math.Max(0, (processedAtUtc - enqueuedAtUtc).TotalMilliseconds);
        RecordStep("rabbitmq", model, "queue_latency", latencyMs, entityId, "ok");

        var rateKey = $"queue_rate:{model}";
        var bucket = _rateSamples.GetOrAdd(rateKey, _ => new ConcurrentQueue<DateTime>());
        bucket.Enqueue(processedAtUtc);

        var cutoff = processedAtUtc.AddSeconds(-60);
        while (bucket.TryPeek(out var oldest) && oldest < cutoff)
        {
            bucket.TryDequeue(out _);
        }

        var ratePerSecond = bucket.Count / 60d;
        _logger.LogInformation(
            "[Timing] pipeline=rabbitmq model={Model} step=queue_rate_60s entityId={EntityId} itemsInLast60s={Count} ratePerSecond={RatePerSecond} latencyMs={LatencyMs}",
            model,
            entityId ?? "n/a",
            bucket.Count,
            ratePerSecond,
            latencyMs);

        LogQueueSummary(model, "queue_rate_60s", bucket.Count, ratePerSecond, latencyMs);
    }

    public void RecordQueuePublish(string model, DateTime issuedAtUtc)
    {
        var rateKey = $"queue_publish:{model}";
        var bucket = _rateSamples.GetOrAdd(rateKey, _ => new ConcurrentQueue<DateTime>());
        bucket.Enqueue(issuedAtUtc);

        var cutoff = issuedAtUtc.AddSeconds(-60);
        while (bucket.TryPeek(out var oldest) && oldest < cutoff)
        {
            bucket.TryDequeue(out _);
        }

        var ratePerSecond = bucket.Count / 60d;
        _logger.LogInformation(
            "[Timing] pipeline=rabbitmq model={Model} step=publish_rate_60s itemsInLast60s={Count} ratePerSecond={RatePerSecond}",
            model,
            bucket.Count,
            ratePerSecond);

        LogQueueSummary(model, "publish_rate_60s", bucket.Count, ratePerSecond, null);
    }

    public void LogSummarySnapshot(string? pipeline = null, string? model = null, string? step = null)
    {
        var summaries = new List<object>();

        foreach (var entry in _durationSamples)
        {
            var parts = entry.Key.Split(':', 3);
            var entryPipeline = parts.Length > 0 ? parts[0] : string.Empty;
            var entryModel = parts.Length > 1 ? parts[1] : string.Empty;
            var entryStep = parts.Length > 2 ? parts[2] : string.Empty;

            if (pipeline is not null && !string.Equals(entryPipeline, pipeline, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (model is not null && !string.Equals(entryModel, model, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (step is not null && !string.Equals(entryStep, step, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var values = entry.Value.ToArray();
            if (values.Length == 0)
            {
                continue;
            }

            var avgMs = values.Average();
            var minMs = values.Min();
            var maxMs = values.Max();
            var p95Ms = GetPercentile(values, 95);

            summaries.Add(new
            {
                pipeline = entryPipeline,
                model = entryModel,
                step = entryStep,
                count = values.Length,
                avgMs,
                minMs,
                maxMs,
                p95Ms,
                lastMs = values[^1]
            });
        }

        var payload = new
        {
            generatedAtUtc = DateTime.UtcNow,
            metrics = summaries
        };

        _logger.LogInformation("[Timing Summary] {SummaryJson}", JsonSerializer.Serialize(payload));
    }

    private void LogQueueSummary(string model, string step, int sampleCount, double ratePerSecond, long? latencyMs)
    {
        var payload = new
        {
            generatedAtUtc = DateTime.UtcNow,
            pipeline = "rabbitmq",
            model,
            step,
            sampleCount,
            ratePerSecond,
            latencyMs
        };

        _logger.LogInformation("[Timing Summary] {SummaryJson}", JsonSerializer.Serialize(payload));
        WriteDashboardSummary();
    }

    private void WriteDashboardSummary()
    {
        var report = BuildDashboardSummary();
        var drilldown = BuildDrilldownText();

        lock (_summaryFileLock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_summaryFilePath)!);
            File.WriteAllText(_summaryFilePath, report);
            File.WriteAllText(_drilldownFilePath, drilldown);
        }
    }

    private string BuildDashboardSummary()
    {
        var entries = GetSummaryEntries();
        if (entries.Count == 0)
        {
            return "# BlockiFinAid Timing Dashboard\n\nNo timing samples have been recorded yet.\n";
        }

        var byModel = entries
            .GroupBy(x => x.model)
            .Select(g => new
            {
                model = g.Key,
                totalSamples = g.Sum(x => x.count),
                avgMs = g.Where(x => x.step.Equals("total_processing", StringComparison.OrdinalIgnoreCase)).Select(x => x.avgMs).DefaultIfEmpty(0).Average(),
                p95Ms = g.Where(x => x.step.Equals("total_processing", StringComparison.OrdinalIgnoreCase)).Select(x => x.p95Ms).DefaultIfEmpty(0).Max()
            })
            .OrderBy(x => x.model)
            .ToList();

        var totalOperations = entries.Sum(x => x.count);
        var avgOverall = entries.Average(x => x.avgMs);
        var p95Overall = entries.Max(x => x.p95Ms);
        var topSlowest = entries.OrderByDescending(x => x.avgMs).Take(10).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# BlockiFinAid Timing Dashboard");
        sb.AppendLine($"> Generated UTC: {DateTime.UtcNow:O}");
        sb.AppendLine();
        sb.AppendLine("## Executive Summary");
        sb.AppendLine($"- Total observed operations: {totalOperations}");
        sb.AppendLine($"- Average duration across all recorded steps: {avgOverall:F1} ms");
        sb.AppendLine($"- Peak p95 duration recorded: {p95Overall} ms");
        sb.AppendLine($"- Models tracked: {byModel.Count}");
        sb.AppendLine();
        sb.AppendLine("## End-to-End by Model");
        sb.AppendLine("| Model | Samples | Avg total processing | P95 total processing |");
        sb.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var model in byModel)
        {
            sb.AppendLine($"| {model.model} | {model.totalSamples} | {model.avgMs:F1} ms | {model.p95Ms} ms |");
        }

        sb.AppendLine();
        sb.AppendLine("## Slowest Average Stages");
        sb.AppendLine("| Pipeline | Model | Step | Count | Avg (ms) | P95 (ms) |");
        sb.AppendLine("| --- | --- | --- | ---: | ---: | ---: |");

        foreach (var item in topSlowest)
        {
            sb.AppendLine($"| {item.pipeline} | {item.model} | {item.step} | {item.count} | {item.avgMs:F1} | {item.p95Ms} |");
        }

        sb.AppendLine();
        sb.AppendLine("## Drill-down Summary");
        sb.AppendLine("| Pipeline | Model | Step | Count | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |");
        sb.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |");

        foreach (var item in entries.OrderBy(x => x.pipeline).ThenBy(x => x.model).ThenBy(x => x.step))
        {
            sb.AppendLine($"| {item.pipeline} | {item.model} | {item.step} | {item.count} | {item.avgMs:F1} | {item.minMs} | {item.maxMs} | {item.p95Ms} |");
        }

        return sb.ToString();
    }

    private string BuildDrilldownText()
    {
        var entries = GetSummaryEntries();
        if (entries.Count == 0)
        {
            return "No timing samples recorded yet.\n";
        }

        var sb = new StringBuilder();
        sb.AppendLine("BlockiFinAid Timing Drill-down");
        sb.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
        sb.AppendLine();

        foreach (var item in entries.OrderBy(x => x.pipeline).ThenBy(x => x.model).ThenBy(x => x.step))
        {
            sb.AppendLine($"- pipeline={item.pipeline} model={item.model} step={item.step} count={item.count} avgMs={item.avgMs:F1} minMs={item.minMs} maxMs={item.maxMs} p95Ms={item.p95Ms} lastMs={item.lastMs}");
        }

        return sb.ToString();
    }

    private List<(string pipeline, string model, string step, int count, double avgMs, long minMs, long maxMs, long p95Ms, long lastMs)> GetSummaryEntries()
    {
        var summaries = new List<(string pipeline, string model, string step, int count, double avgMs, long minMs, long maxMs, long p95Ms, long lastMs)>();

        foreach (var entry in _durationSamples)
        {
            var parts = entry.Key.Split(':', 3);
            var pipeline = parts.Length > 0 ? parts[0] : string.Empty;
            var model = parts.Length > 1 ? parts[1] : string.Empty;
            var step = parts.Length > 2 ? parts[2] : string.Empty;
            var values = entry.Value.ToArray();

            if (values.Length == 0)
            {
                continue;
            }

            summaries.Add((pipeline, model, step, values.Length, values.Average(), values.Min(), values.Max(), GetPercentile(values, 95), values[^1]));
        }

        return summaries;
    }

    private static long GetPercentile(long[] values, int percentile)
    {
        if (values.Length == 0)
        {
            return 0;
        }

        var sorted = values.OrderBy(x => x).ToArray();
        var index = (int)Math.Ceiling((percentile / 100d) * sorted.Length) - 1;
        index = Math.Max(0, Math.Min(sorted.Length - 1, index));
        return sorted[index];
    }

    private readonly record struct HttpEndpointKey(string Method, string Route, int StatusCode);
}
