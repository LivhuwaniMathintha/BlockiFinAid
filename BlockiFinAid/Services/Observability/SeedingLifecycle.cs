namespace BlockiFinAid.Services.Observability;

public sealed class SeedingLifecycle
{
    private readonly TaskCompletionSource<bool> _seedCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitUntilCompleteAsync(CancellationToken cancellationToken = default)
    {
        return _seedCompleted.Task.WaitAsync(cancellationToken);
    }

    public void MarkComplete()
    {
        _seedCompleted.TrySetResult(true);
    }
}
