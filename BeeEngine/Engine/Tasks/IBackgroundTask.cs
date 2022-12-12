namespace BeeEngine
{
    public interface IBackgroundTask
    {
        void Start();
        Task StopAsync();
    }
}