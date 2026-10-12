namespace BlockiFinAid.Services.Observability;

public sealed class DemoRunMetrics
{
    private readonly object _lock = new();
    private DemoRunSnapshot? _currentRun;

    public DemoRunSnapshot? GetCurrentRun()
    {
        lock (_lock)
        {
            return _currentRun;
        }
    }

    public void SetCurrentRun(string runId, IReadOnlyDictionary<string, long> countsByFunder)
    {
        lock (_lock)
        {
            _currentRun = new DemoRunSnapshot(
                runId,
                countsByFunder.ToDictionary(pair => pair.Key, pair => pair.Value));
        }
    }
}

public sealed record DemoRunSnapshot(
    string RunId,
    IReadOnlyDictionary<string, long> CountsByFunder);
