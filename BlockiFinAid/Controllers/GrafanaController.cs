using System.Globalization;
using System.Text;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Observability;
using BlockiFinAid.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace BlockiFinAid.Controllers;

[ApiController]
[Route("api/grafana")]
public sealed class GrafanaController : ControllerBase
{
    private readonly IBaseRepository<FunderModel> _funderRepository;
    private readonly IMongoDatabase _fundsDatabase;
    private readonly IMongoDatabase _institutionDatabase;
    private readonly IMongoDatabase _accessDatabase;
    private readonly IConfiguration _configuration;
    private readonly OperationTimingTracker _timingTracker;
    private readonly DemoRunMetrics _demoRunMetrics;
    private readonly ILogger<GrafanaController> _logger;

    public GrafanaController(
        IBaseRepository<FunderModel> funderRepository,
        [FromKeyedServices("DB.Funds")] IMongoDatabase fundsDatabase,
        [FromKeyedServices("DB.Institution")] IMongoDatabase institutionDatabase,
        [FromKeyedServices("DB.UserAccessControl")] IMongoDatabase accessDatabase,
        IConfiguration configuration,
        OperationTimingTracker timingTracker,
        DemoRunMetrics demoRunMetrics,
        ILogger<GrafanaController> logger)
    {
        _funderRepository = funderRepository;
        _fundsDatabase = fundsDatabase;
        _institutionDatabase = institutionDatabase;
        _accessDatabase = accessDatabase;
        _configuration = configuration;
        _timingTracker = timingTracker;
        _demoRunMetrics = demoRunMetrics;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        return Ok(await BuildOverviewAsync());
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var health = await BuildHealthAsync(cancellationToken);
        return StatusCode(health.IsHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable, health);
    }

    [HttpGet("timings")]
    public IActionResult GetTimings()
    {
        return Ok(new
        {
            generatedAtUtc = DateTime.UtcNow,
            backgroundOperations = _timingTracker.GetTimingSummaries(),
            httpRequests = _timingTracker.GetHttpRequestSummaries()
        });
    }

    [HttpGet("metrics")]
    [Produces("text/plain")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var health = await BuildHealthAsync(cancellationToken);
        var output = new StringBuilder();
        AddMetric(output, "blockifinaid_api_up", "Whether the BlockiFinAid API is responding.", "gauge", 1);
        AddMetric(output, "blockifinaid_dependency_up", "Whether a backing dependency is reachable.", "gauge");

        foreach (var dependency in health.Dependencies)
        {
            AddMetricSample(
                output,
                "blockifinaid_dependency_up",
                new Dictionary<string, string> { ["dependency"] = dependency.Name },
                dependency.IsHealthy ? 1 : 0);
        }

        if (health.Dependencies
            .Where(dependency => dependency.Name is "MongoDB_Funds" or "MongoDB_Institution")
            .All(dependency => dependency.IsHealthy))
        {
            var overview = await BuildOverviewAsync();
            AppendOverviewMetrics(output, overview);
        }

        AddMetric(output, "blockifinaid_demo_run_students_funded", "Students funded by each funder in the most recent demo run.", "gauge");
        AddMetric(output, "blockifinaid_demo_run_payments_created", "Payments created by each funder in the most recent demo run.", "gauge");
        var demoRun = _demoRunMetrics.GetCurrentRun();
        if (demoRun is not null)
        {
            foreach (var (funder, count) in demoRun.CountsByFunder)
            {
                var labels = new Dictionary<string, string>
                {
                    ["funder"] = funder,
                    ["demo_run_id"] = demoRun.RunId
                };
                AddMetricSample(output, "blockifinaid_demo_run_students_funded", labels, count);
                AddMetricSample(output, "blockifinaid_demo_run_payments_created", labels, count);
            }
        }

        AddMetric(output, "blockifinaid_operation_duration_ms_avg", "Average duration of recent recorded background operation samples.", "gauge");
        AddMetric(output, "blockifinaid_operation_duration_ms_p95", "P95 duration of recent recorded background operation samples.", "gauge");
        foreach (var timing in _timingTracker.GetTimingSummaries())
        {
            var labels = new Dictionary<string, string>
            {
                ["pipeline"] = timing.Pipeline,
                ["model"] = timing.Model,
                ["step"] = timing.Step
            };
            AddMetricSample(output, "blockifinaid_operation_duration_ms_avg", labels, timing.AverageMs);
            AddMetricSample(output, "blockifinaid_operation_duration_ms_p95", labels, timing.P95Ms);
        }

        AddMetric(output, "blockifinaid_http_requests_total", "Total HTTP requests observed since API start.", "counter");
        AddMetric(output, "blockifinaid_http_request_duration_ms_avg", "Average duration of the most recent HTTP request samples.", "gauge");
        AddMetric(output, "blockifinaid_http_request_duration_ms_p95", "P95 duration of the most recent HTTP request samples.", "gauge");
        foreach (var request in _timingTracker.GetHttpRequestSummaries())
        {
            var labels = new Dictionary<string, string>
            {
                ["method"] = request.Method,
                ["route"] = request.Route,
                ["status"] = request.StatusCode.ToString(CultureInfo.InvariantCulture)
            };
            AddMetricSample(output, "blockifinaid_http_requests_total", labels, request.Count);
            AddMetricSample(output, "blockifinaid_http_request_duration_ms_avg", labels, request.AverageMs);
            AddMetricSample(output, "blockifinaid_http_request_duration_ms_p95", labels, request.P95Ms);
        }

        return Content(output.ToString(), "text/plain; version=0.0.4; charset=utf-8");
    }

    private async Task<OverviewSnapshot> BuildOverviewAsync()
    {
        var fundersTask = _funderRepository.GetAllAsync();
        var studentFilter = Builders<UserModel>.Filter.And(
            Builders<UserModel>.Filter.Regex(user => user.Role, new BsonRegularExpression("^student$", "i")),
            Builders<UserModel>.Filter.Eq(user => user.IsActive, true));
        var studentsTask = _institutionDatabase.GetCollection<UserModel>("users")
            .Find(studentFilter)
            .Project(user => new { user.Id, user.FunderContractId, user.DataSource })
            .ToListAsync();
        var activeFundingsTask = _fundsDatabase.GetCollection<FundingModel>("funding")
            .Find(Builders<FundingModel>.Filter.Eq(funding => funding.IsActive, true))
            .Project(funding => new { funding.StudentId, funding.FunderId, funding.DataSource })
            .ToListAsync();
        var initiatedSince = DateTime.UtcNow.AddHours(-24);
        var paymentsCollection = _fundsDatabase.GetCollection<PaymentModel>("payments");
        var persistedPaymentGroupsTask = paymentsCollection
            .Aggregate()
            .Group(
                payment => new
                {
                    payment.Status,
                    payment.Funder,
                    payment.DataSource,
                },
                group => new
                {
                    Status = group.Key.Status,
                    Funder = group.Key.Funder,
                    DataSource = group.Key.DataSource,
                    Count = group.Count(),
                    AmountTotal = group.Sum(payment => (long)payment.Amount)
                })
            .ToListAsync();
        var recentPaymentGroupsTask = paymentsCollection
            .Aggregate()
            .Match(Builders<PaymentModel>.Filter.Gte(payment => payment.InitiationDate, initiatedSince))
            .Group(
                payment => new
                {
                    payment.Status,
                    payment.Funder,
                    payment.DataSource,
                    payment.FailureReason,
                    payment.DemoRunId
                },
                group => new
                {
                    Status = group.Key.Status,
                    Funder = group.Key.Funder,
                    DataSource = group.Key.DataSource,
                    FailureReason = group.Key.FailureReason,
                    DemoRunId = group.Key.DemoRunId,
                    Count = group.Count(),
                    AmountTotal = group.Sum(payment => (long)payment.Amount)
                })
            .ToListAsync();
        var demoRunGroupsTask = paymentsCollection
            .Aggregate()
            .Match(Builders<PaymentModel>.Filter.And(
                Builders<PaymentModel>.Filter.Eq(payment => payment.DataSource, DataSources.Demo),
                Builders<PaymentModel>.Filter.Gte(payment => payment.InitiationDate, initiatedSince)))
            .Group(
                payment => new { payment.DemoRunId, payment.Status, payment.Funder },
                group => new
                {
                    DemoRunId = group.Key.DemoRunId,
                    Status = group.Key.Status,
                    Funder = group.Key.Funder,
                    Count = group.Count()
                })
            .ToListAsync();
        await Task.WhenAll(fundersTask, studentsTask, activeFundingsTask, persistedPaymentGroupsTask, recentPaymentGroupsTask, demoRunGroupsTask);

        var funders = (await fundersTask).ToList();
        var students = await studentsTask;
        var activeFundings = await activeFundingsTask;
        var persistedPaymentGroups = await persistedPaymentGroupsTask;
        var recentPaymentGroups = await recentPaymentGroupsTask;
        var demoRunGroups = await demoRunGroupsTask;
        var fundersById = funders.ToDictionary(funder => funder.Id);
        var studentsById = students.ToDictionary(student => student.Id);
        var fundedStudents = activeFundings
            .Where(funding =>
                studentsById.ContainsKey(funding.StudentId) &&
                fundersById.ContainsKey(funding.FunderId) &&
                studentsById[funding.StudentId].FunderContractId == funding.FunderId)
            .Select(funding =>
            {
                var student = studentsById[funding.StudentId];
                var funder = fundersById[funding.FunderId];
                return new
                {
                    student.Id,
                    FunderId = funder.Id,
                    Funder = funder.Name,
                    StudentDataSource = student.DataSource,
                    FundingDataSource = funding.DataSource
                };
            })
            .GroupBy(student => (student.Id, student.FunderId, student.Funder))
            .Select(group => new
            {
                group.Key.Id,
                group.Key.FunderId,
                group.Key.Funder,
                DataSource = CombineDataSources(
                    group.Select(student => student.StudentDataSource)
                        .Concat(group.Select(student => student.FundingDataSource))
                        .ToArray())
            })
            .GroupBy(student => (student.FunderId, student.Funder, student.DataSource))
            .Select(group => new FundedStudentCount(group.Key.FunderId, group.Key.Funder, group.Key.DataSource, group.LongCount()))
            .OrderBy(group => group.Funder)
            .ThenBy(group => group.DataSource)
            .ToList();

        var fundedStudentCounts = funders
            .SelectMany(funder => new[] { DataSources.Live, DataSources.Seed, DataSources.Demo }
                .Select(source =>
                {
                    var existing = fundedStudents.FirstOrDefault(student =>
                        student.FunderId == funder.Id && student.DataSource == source);
                    return existing ?? new FundedStudentCount(funder.Id, funder.Name, source, 0);
                }))
            .ToList();
        var persistedPayments = persistedPaymentGroups
            .Select(payment => new PaymentGroup(
                NormalizeLabel(payment.Funder),
                NormalizeDataSource(payment.DataSource),
                NormalizeLabel(payment.Status),
                string.Empty,
                string.Empty,
                payment.Count,
                payment.AmountTotal))
            .ToList();
        var persistedPaymentDetails = funders
            .SelectMany(funder => new[] { DataSources.Live, DataSources.Seed, DataSources.Demo }
                .SelectMany(source =>
                {
                    var sourcePayments = persistedPayments
                        .Where(payment => payment.Funder == funder.Name && payment.DataSource == source)
                        .ToList();
                    var observedStatuses = sourcePayments.Count == 0
                        ? new[] { PaymentStatuses.Initiated, PaymentStatuses.Processing, PaymentStatuses.Fulfilled, PaymentStatuses.Failed }
                        : sourcePayments.Select(payment => payment.Status).Distinct(StringComparer.OrdinalIgnoreCase);
                    return observedStatuses.Select(status => new PaymentGroup(
                        funder.Name,
                        source,
                        status,
                        string.Empty,
                        string.Empty,
                        sourcePayments
                            .Where(payment => payment.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                            .Sum(payment => payment.Count),
                        0));
                }))
            .ToList();
        var persistedPaymentCounts = funders
            .SelectMany(funder => new[] { DataSources.Live, DataSources.Seed, DataSources.Demo }
                .Select(source =>
                {
                    var existing = persistedPayments
                        .Where(payment => payment.Funder == funder.Name && payment.DataSource == source)
                        .Sum(payment => payment.Count);
                    return new PersistedPaymentCount(funder.Id, funder.Name, source, existing);
                }))
            .ToList();
        var paymentsLast24Hours = recentPaymentGroups
            .Select(payment => new PaymentGroup(
                NormalizeLabel(payment.Funder),
                NormalizeDataSource(payment.DataSource),
                NormalizeLabel(payment.Status),
                NormalizeLabel(payment.FailureReason),
                payment.DemoRunId ?? string.Empty,
                payment.Count,
                payment.AmountTotal))
            .OrderBy(payment => payment.Funder)
            .ThenBy(payment => payment.DataSource)
            .ToList();

        return new OverviewSnapshot(
            DateTime.UtcNow,
            fundedStudentCounts,
            persistedPaymentCounts,
            paymentsLast24Hours,
            persistedPaymentDetails,
            demoRunGroups
                .Select(payment => new DemoPaymentGroup(
                    payment.DemoRunId,
                    NormalizeLabel(payment.Funder),
                    NormalizeLabel(payment.Status),
                    payment.Count))
                .ToList());
    }

    private async Task<HealthSnapshot> BuildHealthAsync(CancellationToken cancellationToken)
    {
        var dependencies = await Task.WhenAll(
            CheckMongoAsync("MongoDB_Funds", _fundsDatabase, cancellationToken),
            CheckMongoAsync("MongoDB_Institution", _institutionDatabase, cancellationToken),
            CheckMongoAsync("MongoDB_Access", _accessDatabase, cancellationToken),
            CheckRabbitMqAsync(cancellationToken));
        return new HealthSnapshot(DateTime.UtcNow, dependencies.All(dependency => dependency.IsHealthy), dependencies);
    }

    private async Task<DependencySnapshot> CheckMongoAsync(
        string name,
        IMongoDatabase database,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        try
        {
            await database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: timeout.Token);
            return new DependencySnapshot(name, true, null);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Health check timed out for {Dependency}.", name);
            return new DependencySnapshot(name, false, "Timeout");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Health check failed for {Dependency}.", name);
            return new DependencySnapshot(name, false, exception.GetType().Name);
        }
    }

    private async Task<DependencySnapshot> CheckRabbitMqAsync(CancellationToken cancellationToken)
    {
        try
        {
            var host = _configuration["RabbitMQ:Host"];
            if (!Uri.TryCreate(host, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException("RabbitMQ:Host must be an absolute AMQP URI.");
            }

            var factory = new ConnectionFactory
            {
                Uri = uri,
                UserName = _configuration["RabbitMQ:Username"] ?? string.Empty,
                Password = _configuration["RabbitMQ:Password"] ?? string.Empty,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
            };
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            return new DependencySnapshot("RabbitMQ", connection.IsOpen, connection.IsOpen ? null : "ConnectionClosed");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Health check failed for RabbitMQ.");
            return new DependencySnapshot("RabbitMQ", false, exception.GetType().Name);
        }
    }

    private static void AppendOverviewMetrics(StringBuilder output, OverviewSnapshot overview)
    {
        AddMetric(output, "blockifinaid_students_funded_total", "Distinct active students with active funding, by funder and data source.", "gauge");
        foreach (var student in overview.FundedStudents)
        {
            AddMetricSample(
                output,
                "blockifinaid_students_funded_total",
                new Dictionary<string, string>
                {
                    ["funder"] = student.Funder,
                    ["funder_id"] = student.FunderId.ToString(),
                    ["data_source"] = student.DataSource
                },
                student.Count);
        }

        AddMetric(output, "blockifinaid_payments_persisted_total", "All persisted payment records by funder and data source.", "gauge");
        AddMetric(output, "blockifinaid_payments_persisted_by_status_total", "All persisted payment records grouped by status, funder, and data source.", "gauge");
        foreach (var payment in overview.PersistedPayments)
        {
            AddMetricSample(
                output,
                "blockifinaid_payments_persisted_total",
                new Dictionary<string, string>
                {
                    ["funder"] = payment.Funder,
                    ["funder_id"] = payment.FunderId.ToString(),
                    ["data_source"] = payment.DataSource
                },
                payment.Count);
        }

        foreach (var payment in overview.PersistedPaymentDetails)
        {
            AddMetricSample(
                output,
                "blockifinaid_payments_persisted_by_status_total",
                new Dictionary<string, string>
                {
                    ["funder"] = payment.Funder,
                    ["status"] = payment.Status,
                    ["data_source"] = payment.DataSource
                },
                payment.Count);
        }

        AddMetric(output, "blockifinaid_payments_created_last_24_hours", "Payments initiated during the last 24 hours, by funder and data source.", "gauge");
        AddMetric(output, "blockifinaid_payment_status_last_24_hours", "Payments initiated during the last 24 hours, grouped by status and data source.", "gauge");
        AddMetric(output, "blockifinaid_payment_failures_last_24_hours", "Failed payments initiated during the last 24 hours, by recorded reason and data source.", "gauge");
        AddMetric(output, "blockifinaid_demo_payments_total", "Tagged demo payment records initiated during the last 24 hours, by run, funder, and current status.", "gauge");
        var recentPaymentsByFunder = overview.PaymentsLast24Hours
            .GroupBy(payment => (payment.Funder, payment.DataSource))
            .ToDictionary(group => group.Key, group => group.Sum(payment => payment.Count));
        var recentStatusesByFunder = overview.PaymentsLast24Hours
            .GroupBy(payment => (payment.Funder, payment.DataSource, payment.Status))
            .ToDictionary(group => group.Key, group => group.Sum(payment => payment.Count));
        foreach (var funder in overview.FundedStudents
                     .Select(student => (student.Funder, student.FunderId))
                     .Distinct())
        {
            foreach (var source in new[] { DataSources.Live, DataSources.Seed, DataSources.Demo })
            {
                AddMetricSample(
                    output,
                    "blockifinaid_payments_created_last_24_hours",
                    new Dictionary<string, string>
                    {
                        ["funder"] = funder.Funder,
                        ["data_source"] = source
                    },
                    recentPaymentsByFunder.GetValueOrDefault((funder.Funder, source)));

                foreach (var status in new[]
                         {
                             PaymentStatuses.Initiated,
                             PaymentStatuses.Processing,
                             PaymentStatuses.Fulfilled,
                             PaymentStatuses.Failed,
                             PaymentStatuses.FraudCheckFailed,
                             PaymentStatuses.FraudRejected,
                             PaymentStatuses.FraudCheckPassed,
                             PaymentStatuses.RecordedOnChain
                         })
                {
                    AddMetricSample(
                        output,
                        "blockifinaid_payment_status_last_24_hours",
                        new Dictionary<string, string>
                        {
                            ["funder"] = funder.Funder,
                            ["status"] = status,
                            ["data_source"] = source
                        },
                        recentStatusesByFunder.GetValueOrDefault((funder.Funder, source, status)));
                }
            }
        }

        foreach (var payment in overview.PaymentsLast24Hours
                     .Where(payment => IsFailedStatus(payment.Status))
                     .GroupBy(payment => (payment.Funder, payment.FailureReason, payment.DataSource))
                     .Select(group => new { group.Key.Funder, group.Key.FailureReason, group.Key.DataSource, Count = group.Sum(payment => payment.Count) }))
        {
            AddMetricSample(
                output,
                "blockifinaid_payment_failures_last_24_hours",
                new Dictionary<string, string>
                {
                    ["funder"] = payment.Funder,
                    ["reason"] = payment.FailureReason,
                    ["data_source"] = payment.DataSource
                },
                payment.Count);
        }

        foreach (var demoPayment in overview.DemoRuns)
        {
            AddMetricSample(
                output,
                "blockifinaid_demo_payments_total",
                new Dictionary<string, string>
                {
                    ["demo_run_id"] = demoPayment.RunId,
                    ["funder"] = demoPayment.Funder,
                    ["status"] = demoPayment.Status,
                    ["data_source"] = DataSources.Demo
                },
                demoPayment.Count);
        }
    }

    private static string NormalizeLabel(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "(unknown)" : value.Trim();
    }

    private static bool IsFailedStatus(string status)
    {
        return status.Equals(PaymentStatuses.Failed, StringComparison.OrdinalIgnoreCase) ||
               status.Equals(PaymentStatuses.FraudCheckFailed, StringComparison.OrdinalIgnoreCase) ||
               status.Equals(PaymentStatuses.FraudRejected, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeDataSource(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            DataSources.Demo => DataSources.Demo,
            DataSources.Seed => DataSources.Seed,
            _ => DataSources.Live
        };
    }

    private static string CombineDataSources(params string?[] sources)
    {
        if (sources.Any(source => string.Equals(source, DataSources.Demo, StringComparison.OrdinalIgnoreCase)))
        {
            return DataSources.Demo;
        }

        return sources.Any(source => string.Equals(source, DataSources.Seed, StringComparison.OrdinalIgnoreCase))
            ? DataSources.Seed
            : DataSources.Live;
    }

    private static void AddMetric(StringBuilder output, string name, string help, string type, double? value = null)
    {
        output.Append("# HELP ").Append(name).Append(' ').AppendLine(help);
        output.Append("# TYPE ").Append(name).Append(' ').AppendLine(type);
        if (value.HasValue)
        {
            output.Append(name).Append(' ').AppendLine(FormatValue(value.Value));
        }
    }

    private static void AddMetricSample(
        StringBuilder output,
        string name,
        IReadOnlyDictionary<string, string> labels,
        double value)
    {
        output.Append(name).Append('{');
        output.AppendJoin(',', labels.Select(label =>
            $"{label.Key}=\"{EscapeLabelValue(label.Value)}\""));
        output.Append("} ").AppendLine(FormatValue(value));
    }

    private static string EscapeLabelValue(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }

    private static string FormatValue(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private sealed record OverviewSnapshot(
        DateTime GeneratedAtUtc,
        IReadOnlyList<FundedStudentCount> FundedStudents,
        IReadOnlyList<PersistedPaymentCount> PersistedPayments,
        IReadOnlyList<PaymentGroup> PaymentsLast24Hours,
        IReadOnlyList<PaymentGroup> PersistedPaymentDetails,
        IReadOnlyList<DemoPaymentGroup> DemoRuns);
    private sealed record FundedStudentCount(Guid FunderId, string Funder, string DataSource, long Count);
    private sealed record PersistedPaymentCount(Guid FunderId, string Funder, string DataSource, long Count);
    private sealed record PaymentGroup(string Funder, string DataSource, string Status, string FailureReason, string DemoRunId, long Count, long AmountTotal);
    private sealed record DemoPaymentGroup(string RunId, string Funder, string Status, long Count);
    private sealed record DependencySnapshot(string Name, bool IsHealthy, string? Error);
    private sealed record HealthSnapshot(DateTime GeneratedAtUtc, bool IsHealthy, IReadOnlyList<DependencySnapshot> Dependencies);
}
