using BeeEngine.DependencyInjection;

namespace BeeEngine.Mediator
{
    public static class MediatorExtensions
    {
        public static DIServiceCollection AddMediator(
            this DIServiceCollection services, 
            DILifeTime lifeTime, 
            params Type[] markers)
        {
            var handlerInfo = new Dictionary<Type, Type>();
            var notificationHandlerInfo = new Dictionary<Type, List<Type>>();
            foreach (Type marker in markers)
            {
                ProcessAssemblyAndFindRequestsAndHandlers(services, lifeTime, marker, handlerInfo, notificationHandlerInfo);
            }
            services.AddSingleton<IMediator>(new Mediator(services.BuildServiceProvider().GetRequiredService, handlerInfo, notificationHandlerInfo));
            return services;
        }
        public static DIServiceCollection AddMediator(
            this DIServiceCollection services, 
            params Type[] markers)
        {
            var handlerInfo = new Dictionary<Type, Type>();
            var notificationHandlerInfo = new Dictionary<Type, List<Type>>();
            foreach (Type marker in markers)
            {
                ProcessAssemblyAndFindRequestsAndHandlers(services, DILifeTime.Transient, marker, handlerInfo, notificationHandlerInfo);
            }
            services.AddSingleton<IMediator>(new Mediator(services.BuildServiceProvider().GetRequiredService, handlerInfo, notificationHandlerInfo));
            return services;
        }
        public static DIServiceCollection AddMediator<TMarker>(
            this DIServiceCollection services, 
            DILifeTime lifeTime)
        {
            var handlerInfo = new Dictionary<Type, Type>();
            var notificationHandlerInfo = new Dictionary<Type, List<Type>>();
            ProcessAssemblyAndFindRequestsAndHandlers(services, lifeTime, typeof(TMarker), handlerInfo, notificationHandlerInfo);
            services.AddSingleton<IMediator>(new Mediator(services.BuildServiceProvider().GetRequiredService, handlerInfo, notificationHandlerInfo));
            return services;
        }
        public static DIServiceCollection AddMediator<TMarker>(
            this DIServiceCollection services)
        {
            var handlerInfo = new Dictionary<Type, Type>();
            var notificationHandlerInfo = new Dictionary<Type, List<Type>>();
            ProcessAssemblyAndFindRequestsAndHandlers(services, DILifeTime.Transient, typeof(TMarker), handlerInfo, notificationHandlerInfo);
            services.AddSingleton<IMediator>(new Mediator(services.BuildServiceProvider().GetRequiredService, handlerInfo, notificationHandlerInfo));
            return services;
        }

        private static void ProcessAssemblyAndFindRequestsAndHandlers(DIServiceCollection services, DILifeTime lifeTime,
            Type marker, Dictionary<Type, Type> handlerInfo, Dictionary<Type, List<Type>> notificationHandlerInfo)
        {
            var assembly = marker.Assembly;
            var requests = GetClassesImplementingInterface(assembly, typeof(IRequest<>));
            var handlers = GetClassesImplementingInterface(assembly, typeof(IRequestHandler<,>));
            var notifications = GetClassesImplementingInterface(assembly, typeof(INotification));
            var notificationHandlers = GetClassesImplementingInterface(assembly, typeof(INotificationHandler<>));
            requests.ForEach(x =>
            {
                handlerInfo[x] =
                    handlers.SingleOrDefault(xx => x == xx.GetInterface("IRequestHandler`2")!.GetGenericArguments()[0])!;
            });
            notifications.ForEach(x =>
            {
                notificationHandlerInfo[x] = notificationHandlers.FindAll(xx => x == xx.GetInterface
                    ("INotificationHandler`1")!.GetGenericArguments()[0]).ToList();
            });
            var serviceDescriptors = handlers.Select(x => new DIServiceDescriptor(x, lifeTime)).Concat
                (notificationHandlers.Select(x => new DIServiceDescriptor(x, lifeTime)));
            services.TryAdd(serviceDescriptors);
        }

        private static List<Type> GetClassesImplementingInterface(Assembly assembly, Type typeToMatch)
        {
            return assembly.ExportedTypes
                .Where(type =>
                {
                    var genericInterfaceTypes = type.GetInterfaces().Where(x => x.IsGenericType);
                    var implementRequestType = genericInterfaceTypes.Any(x => x.GetGenericTypeDefinition() ==
                                                                              typeToMatch);
                    return !type.IsInterface && !type.IsAbstract && implementRequestType;
                }).ToList();
        }
    }
}