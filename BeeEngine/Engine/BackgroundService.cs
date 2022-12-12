namespace BeeEngine
{
    public abstract class BackgroundService
    {
        public abstract Task WorkAsync(CancellationToken token);
    }
}