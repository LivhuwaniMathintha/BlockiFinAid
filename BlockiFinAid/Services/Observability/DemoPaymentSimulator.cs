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
    private readonly DemoRunMetrics _demoRunMetrics;
    private readonly ILogger<DemoPaymentSimulator> _logger;

    public DemoPaymentSimulator(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        SeedingLifecycle seedingLifecycle,
        DemoRunMetrics demoRunMetrics,
        ILogger<DemoPaymentSimulator> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _seedingLifecycle = seedingLifecycle;
        _demoRunMetrics = demoRunMetrics;
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
        var userRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<UserModel>>();
        var fundingRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<FundingModel>>();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<PaymentModel>>();
        var activeFunders = funders.Where(funder => funder.IsActive).ToList();
        if (activeFunders.Count == 0)
        {
            throw new InvalidOperationException("Demo simulation requires at least one active funder.");
        }

        var minStudents = _configuration.GetValue("DemoSimulation:MinStudentsPerRun", 50);
        var maxStudents = _configuration.GetValue("DemoSimulation:MaxStudentsPerRun", 150);
        if (minStudents is < 50 or > 150 || maxStudents is < 50 or > 150 || minStudents > maxStudents)
        {
            throw new InvalidOperationException(
                "DemoSimulation student limits must satisfy 50 <= MinStudentsPerRun <= MaxStudentsPerRun <= 150.");
        }

        var studentCount = Random.Shared.Next(minStudents, maxStudents + 1);
        var runId = Guid.NewGuid().ToString("N");
        var random = Random.Shared;
        var institutionId = users
            .Where(user => user.InstitutionId != Guid.Empty)
            .Select(user => user.InstitutionId)
            .FirstOrDefault();
        var pending = new List<SimulatedPayment>(studentCount);
        var studentsByFunder = activeFunders.ToDictionary(funder => funder.Name, _ => 0L);
        for (var index = 0; index < studentCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var funder = activeFunders[random.Next(activeFunders.Count)];
            var studentId = Guid.NewGuid();
            var studentNumber = $"DEMO-{runId[..8]}-{index + 1:D3}";
            var now = DateTime.UtcNow;
            var student = new UserModel
            {
                Id = studentId,
                InstitutionId = institutionId,
                FunderContractId = funder.Id,
                BankAccountId = Guid.NewGuid(),
                StudentNumber = studentNumber,
                Name = $"Demo Student {runId[..8]}-{index + 1:D3}",
                Email = $"demo-{runId}-{index + 1:D3}@blockifinaid.invalid",
                IsActive = true,
                IsChangeConfirmed = true,
                ModifiedBy = "demo-simulator",
                UpdatedAt = now.ToString("O"),
                CourseName = "Demo course",
                Role = "student",
                DataSource = DataSources.Demo
            };
            var createdStudent = await userRepository.CreateAsync(student, "demo-simulator");
            if (createdStudent is not UserModel)
            {
                throw new InvalidOperationException(
                    $"Could not persist demo student {student.Id}; repository returned {createdStudent?.GetType().Name ?? "null"}.");
            }

            var funding = new FundingModel
            {
                Id = Guid.NewGuid(),
                FunderId = funder.Id,
                StudentId = student.Id,
                SignedOn = now,
                IsActive = true,
                DataSource = DataSources.Demo,
                FoodBalance = (uint)random.Next(500, 5001),
                TuitionBalance = (uint)random.Next(5000, 25001),
                LaptopBalance = (uint)random.Next(0, 5001),
                AccommodationBalance = (uint)random.Next(1000, 10001),
                ModifiedBy = "demo-simulator",
                UpdatedAt = now
            };
            var createdFunding = await fundingRepository.CreateAsync(funding, "demo-simulator");
            if (createdFunding is not FundingModel)
            {
                throw new InvalidOperationException(
                    $"Could not persist demo funding {funding.Id}; repository returned {createdFunding?.GetType().Name ?? "null"}.");
            }

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

            studentsByFunder[funder.Name]++;
            var shouldFail = index == studentCount - 1 ||
                             (index > 0 && random.NextDouble() < 0.25);
            var failureReason = shouldFail
                ? SimulatedFailureReasons[random.Next(SimulatedFailureReasons.Length)]
                : string.Empty;
            pending.Add(new SimulatedPayment(payment, shouldFail ? PaymentStatuses.Failed : PaymentStatuses.Fulfilled, failureReason));
        }

        _demoRunMetrics.SetCurrentRun(runId, studentsByFunder);
        _logger.LogInformation(
            "Created {StudentCount} tagged demo students and one payment per student for run {DemoRunId}. No real payment, fraud-check, or blockchain processing was performed.",
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
