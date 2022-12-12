namespace BeeEngine.Tasks;

public sealed class BackgroundTask : IBackgroundTask
{
    private readonly PeriodicTimer _timer;
    private Task? _task;
    private readonly Action _func;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public BackgroundTask(TimeSpan interval, Action func)
    {
        _func = func;
        _timer = new PeriodicTimer(interval);
    }
    public BackgroundTask(Action func)
    {
        _func = func;
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));
    }

    public void Start()
    {
        _task = DoWorkAsync();
    }

    private async Task DoWorkAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
            {
                _func.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task StopAsync()
    {
        if (_task == null)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
        await _task;
        _cancellationTokenSource.Dispose();
        return;
    }
}