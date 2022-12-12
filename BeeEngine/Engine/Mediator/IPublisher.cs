namespace BeeEngine.Mediator
{
    public interface IPublisher
    {
        Task Publish(INotification notification);
    }
}