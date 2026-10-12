using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Repository;

namespace BlockiFinAid.Services.Observability;

public sealed class DemoPaymentSimulator : BackgroundService
{
    private static readonly string[] SimulatedFailureReasons =
    [
        "Simulated blockchain unavailable",
        "Simulated database unavailable",
        "Simulated fraud-check timeout"
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly SeedingLifecycle _seedingLifecycle;
    private readonly ILogger<DemoPaymentSimulator> _logger;

    public DemoPaymentSimulator(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        SeedingLifecycle seedingLifecycle,
        ILogger<DemoPaymentSimulator> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _seedingLifecycle = seedingLifecycle;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue<bool>("DemoSimulation:Enabled"))
        {
            _logger.LogInformation("Demo payment simulation is disabled.");
            return;
        }

        try
        {
            await _seedingLifecycle.WaitUntilCompleteAsync(stoppingToken);
            await CreateRunAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Demo payment simulation failed.");
        }
    }

    private async Task CreateRunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var users = await scope.ServiceProvider
            .GetRequiredService<IBaseRepository<UserModel>>()
            .GetAllAsync();
        var funders = await scope.ServiceProvider
            .GetRequiredService<IBaseRepository<FunderModel>>()
            .GetAllAsync();
        var fundingRecords = await scope.ServiceProvider
            .GetRequiredService<IBaseRepository<FundingModel>>()
            .GetAllAsync();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<PaymentModel>>();

        var activeStudentFundings = fundingRecords
            .Where(funding => funding.IsActive)
            .Select(funding => (funding.StudentId, funding.FunderId))
            .ToHashSet();
        var fundersById = funders.ToDictionary(funder => funder.Id);
        var eligibleStudents = users
            .Where(user =>
                user.IsActive &&
                string.Equals(user.Role, "student", StringComparison.OrdinalIgnoreCase) &&
                fundersById.ContainsKey(user.FunderContractId) &&
                activeStudentFundings.Contains((user.Id, user.FunderContractId)))
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Clamp(_configuration.GetValue("DemoSimulation:MaxPaymentsPerRun", 20), 1, 100))
            .ToList();

        if (eligibleStudents.Count == 0)
        {
            _logger.LogWarning(
                "Demo payment simulation created no records because there are no active students with matching active funding.");
            return;
        }

        var runId = Guid.NewGuid().ToString("N");
        var random = Random.Shared;
        var pending = new List<SimulatedPayment>(eligibleStudents.Count);
        for (var index = 0; index < eligibleStudents.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var student = eligibleStudents[index];
            var funder = fundersById[student.FunderContractId];
            var payment = new PaymentModel
            {
                Id = Guid.NewGuid(),
                TransactionId = $"DEMO-{runId}-{index + 1}",
                TransactionGroupId = runId,
                AccountNumber = $"DEMO-{Guid.NewGuid():N}",
                StudentId = student.Id,
                StudentNumber = student.StudentNumber,
                Funder = funder.Name,
                Institution = "Demo institution",
                Amount = (uint)random.Next(500, 5001),
                PaymentType = "Food",
                Status = PaymentStatuses.Initiated,
                DataSource = DataSources.Demo,
                DemoRunId = runId,
                InitiationDate = DateTime.UtcNow,
                FulfilmentDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ModifiedBy = "demo-simulator"
            };

            var created = await paymentRepository.CreateAsync(payment, "demo-simulator");
            if (created is not PaymentModel)
            {
                throw new InvalidOperationException(
                    $"Could not persist demo payment {payment.Id}; repository returned {created?.GetType().Name ?? "null"}.");
            }

            var shouldFail = index == eligibleStudents.Count - 1 ||
                             (index > 0 && random.NextDouble() < 0.25);
            var failureReason = shouldFail
                ? SimulatedFailureReasons[random.Next(SimulatedFailureReasons.Length)]
                : string.Empty;
            pending.Add(new SimulatedPayment(payment, shouldFail ? PaymentStatuses.Failed : PaymentStatuses.Fulfilled, failureReason));
        }

        _logger.LogInformation(
            "Created {PaymentCount} tagged demo payments for run {DemoRunId}. No real payment, fraud-check, or blockchain processing was performed.",
            pending.Count,
            runId);

        var stageDelay = TimeSpan.FromSeconds(
            Math.Clamp(_configuration.GetValue("DemoSimulation:StageDelaySeconds", 10), 1, 300));
        await Task.Delay(stageDelay, cancellationToken);

        foreach (var simulatedPayment in pending)
        {
            simulatedPayment.Payment.Status = PaymentStatuses.Processing;
            simulatedPayment.Payment.UpdatedAt = DateTime.UtcNow;
            if (!await paymentRepository.UpdateAsync(simulatedPayment.Payment, "demo-simulator"))
            {
                throw new InvalidOperationException(
                    $"Could not advance demo payment {simulatedPayment.Payment.Id} to {PaymentStatuses.Processing}.");
            }
        }

        await Task.Delay(stageDelay, cancellationToken);

        foreach (var simulatedPayment in pending)
        {
            simulatedPayment.Payment.Status = simulatedPayment.FinalStatus;
            simulatedPayment.Payment.FailureReason = simulatedPayment.FailureReason;
            simulatedPayment.Payment.UpdatedAt = DateTime.UtcNow;
            if (!await paymentRepository.UpdateAsync(simulatedPayment.Payment, "demo-simulator"))
            {
                throw new InvalidOperationException(
                    $"Could not advance demo payment {simulatedPayment.Payment.Id} to {simulatedPayment.FinalStatus}.");
            }
        }

        _logger.LogInformation(
            "Finished demo payment run {DemoRunId}: {FulfilledCount} fulfilled and {FailedCount} failed.",
            runId,
            pending.Count(payment => payment.FinalStatus == PaymentStatuses.Fulfilled),
            pending.Count(payment => payment.FinalStatus == PaymentStatuses.Failed));
    }

    private sealed record SimulatedPayment(PaymentModel Payment, string FinalStatus, string FailureReason);
}
