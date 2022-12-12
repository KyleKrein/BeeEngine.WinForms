namespace BeeEngine.Tasks
{
    public sealed class AsyncThread : IBackgroundTask, IDisposable
    {
        private PeriodicTimer? _timer;
        private Task? _task;
        private readonly Func<CancellationToken, Task>? _func;
        private readonly Func<Task>? _noTokenFunc;
        private CancellationTokenSource _cancellationTokenSource = new();
        private readonly CancellationToken? _userCancellationToken;
        private bool _repeating = false;
        private bool _useCancelationToken = false;
        public bool IsRunning { get; private set; } = false;

        /*public AsyncThread(TimeSpan interval, Func<Task> func)
        {
            _func = func;
            _timer = new PeriodicTimer(interval);
        }*/

        public AsyncThread(Func<CancellationToken, Task> func, CancellationToken token)
        {
            _func = func;
            _userCancellationToken = token;
            _useCancelationToken = true;
        }

        public AsyncThread(Func<Task> func)
        {
            _noTokenFunc = func;
        }

        public AsyncThread Repeat(TimeSpan timeSpan)
        {
            if (_task is not null)
            {
                throw new Exception("Thread is already running. Use ChangeRepeatTime instead");
            }

            _timer = new PeriodicTimer(timeSpan);
            _repeating = true;
            return this;
        }

        public void Start()
        {
            _task = _repeating ? DoRepeatingWorkAsync() : DoWorkAsync();
            /*new TaskFactory().StartNew(_repeating ?
                    async () => await DoRepeatingWorkAsync() : 
                    async () => await DoWorkAsync(),
                TaskCreationOptions.LongRunning);*/
            IsRunning = true;
        }

        public async Task ChangeDelay(TimeSpan timeSpan)
        {
            if (_task is null)
            {
                Repeat(timeSpan);
                return;
            }

            if (!_repeating)
                throw new Exception("Can't change delay on AsyncThread that doesn't repeat a Func");
            await StopAsync();
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new PeriodicTimer(timeSpan);
            Start();
        }

        private async Task DoRepeatingWorkAsync()
        {
            try
            {
                while (await _timer!.WaitForNextTickAsync(_cancellationTokenSource.Token))
                {
                    await _noTokenFunc!.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DoWorkAsync()
        {
            try
            {
                await _noTokenFunc!.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DoRepeatingWorkTokenAsync()
        {
            try
            {
                while (await _timer!.WaitForNextTickAsync(_cancellationTokenSource.Token) &&
                       !_userCancellationToken!.Value.IsCancellationRequested)
                {
                    await _func!.Invoke(_userCancellationToken!.Value);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DoWorkTokenAsync()
        {
            try
            {
                _userCancellationToken!.Value.ThrowIfCancellationRequested();
                await _func!.Invoke(_userCancellationToken!.Value);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task StopAsync()
        {
            if (_task is null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            await _task;
            _cancellationTokenSource.Dispose();
            _timer?.Dispose();
            IsRunning = false;
            return;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _task?.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}