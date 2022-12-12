namespace BeeEngine.Mediator
{
    public interface INotificationHandler<INotification>
    {
        Task Handle(INotification request);
    }
}