namespace BeeEngine.Tasks
{
    public class AsyncBackgroundTask: IBackgroundTask
    {
            private readonly PeriodicTimer _timer;
            private Task? _task;
            private readonly Func<Task> _func;
            private readonly CancellationTokenSource _cancellationTokenSource = new();

            public AsyncBackgroundTask(TimeSpan interval, Func<Task> func)
            {
                _func = func;
                _timer = new PeriodicTimer(interval);
            }
            public AsyncBackgroundTask(Func<Task> func)
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
                        await _func.Invoke();
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
}