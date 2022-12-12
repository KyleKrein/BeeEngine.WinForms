namespace BeeEngine.Tasks
{
    public class FireOnceBackgroundTask : IBackgroundTask
    {
        private readonly PeriodicTimer _timer;
        private Task? _task;
        private readonly Action _func;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public FireOnceBackgroundTask(TimeSpan interval, Action func)
        {
            _func = func;
            _timer = new PeriodicTimer(interval);
        }
        public FireOnceBackgroundTask(Action func)
        {
            _func = func;
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1));
        }

        public void Start()
        {
            _task = DoWorkAsync();
        }

        public async Task StartAsync()
        {
            await DoWorkAsync();
            return;
        }

        private async Task DoWorkAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    _func.Invoke();
                    _cancellationTokenSource.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
            }
            _cancellationTokenSource.Dispose();
            return;
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