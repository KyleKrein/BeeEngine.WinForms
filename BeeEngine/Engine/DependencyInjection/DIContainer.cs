namespace BeeEngine.DependencyInjection
{
    public sealed class DiContainer: IServiceProvider
    {
        private readonly Dictionary<Type, DIServiceDescriptor> _serviceDescriptors;
        internal string? currentScope = null;
        private DiContainer()
        {
        
        }

        internal DiContainer(Dictionary<Type, DIServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;
        }

        public object GetRequiredService(Type serviceType)
        {
            if (!_serviceDescriptors.TryGetValue(serviceType, out var descriptor) || descriptor == null)
                throw new Exception($"Service of {serviceType.Name} is not registered");
            return GetServiceObject(descriptor);
        }

        private object GetServiceObject(DIServiceDescriptor descriptor)
        {
            if (descriptor.Implementation != null || descriptor.LifeTime == DILifeTime.Scoped)
            {
                if (DealWithScopedAndSingleton(descriptor, out object? o))
                {
                    return o!;
                }
            }

            var implementationType = descriptor.ImplementationType;
            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new Exception("Can't create an instance of abstract type or interface");
            }

            var constructorInfo = implementationType.GetConstructors().First();
            var parameters = constructorInfo.GetParameters().Select(x => GetRequiredService(x.ParameterType)).ToArray();
            var implementation = Activator.CreateInstance(implementationType, parameters);
            if (descriptor.LifeTime == DILifeTime.Singleton)
            {
                descriptor.Implementation = implementation;
            }

            return implementation!;
        }

        private bool DealWithScopedAndSingleton(DIServiceDescriptor descriptor, out object? o)
        {
            if (descriptor.LifeTime == DILifeTime.Singleton)
            {
                {
                    o = descriptor.Implementation!;
                    return true;
                }
            }

            if (currentScope == null)
            {
                throw new Exception($"Trying to get {descriptor.ImplementationType.Name} service out of scope.");
            }

            if (descriptor.scope == currentScope)
            {
                o = descriptor.Implementation!;
                return true;
            }

            descriptor.scope = currentScope;
            o = null;
            return false;
        }

        public T GetRequiredService<T>()
        {
            return (T) GetRequiredService(typeof(T));
        }
        public object? GetService(Type serviceType)
        {
            if (!_serviceDescriptors.TryGetValue(serviceType, out var descriptor) || descriptor == null)
            {
                return null;
            }

            return GetServiceObject(descriptor);
        }

        public T? GetService<T>()
        {
            return (T?)GetService(typeof(T));;
        }
    }
}