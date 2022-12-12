using System.Collections.Concurrent;

namespace BeeEngine.Mediator
{
    public sealed class Mediator: IMediator
    {
        private readonly Func<Type, object> _getServiceResolver;
        private readonly ConcurrentDictionary<Type, Type> _handlerDetails;
        private readonly ConcurrentDictionary<Type, List<Type>> _notificationHandlersDetails;
        public Mediator(Func<Type, object> getServiceResolver, IDictionary<Type, Type> handlerDetails, IDictionary<Type, 
        List<Type>> notificationHandlersDetails)
        {
            _getServiceResolver = getServiceResolver;
            _notificationHandlersDetails = new(notificationHandlersDetails);
            _handlerDetails = new(handlerDetails);
        }
        
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var requestType = request.GetType();
            if (!_handlerDetails.TryGetValue(requestType, out var requestHandlerType))
            {
                throw new Exception($"No handler to handle request of type: {requestType.Name}");
            }

            var handler = _getServiceResolver(requestHandlerType);
            return await (Task<TResponse>)handler.GetType().GetMethod("Handle")!
                .Invoke(handler, new[] {request});
        }
        public Task Publish(INotification notification)
        {
            var requestType = notification.GetType();
            if (!_notificationHandlersDetails.TryGetValue(requestType, out var requestHandlerType) || 
            requestHandlerType.Count == 0)
            {
                return Task.CompletedTask;
            }
            var handlers = new List<object>();
            foreach (var v in requestHandlerType)
            {
                var handler = _getServiceResolver(v);
                handler.GetType().GetMethod("HandleNotification")!.Invoke(handler, new[] {notification});
            }
            return Task.CompletedTask;
        }
    }
}