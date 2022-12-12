namespace BeeEngine.DependencyInjection;

public sealed class DIServiceCollection
{
    private Dictionary<Type, DIServiceDescriptor> _serviceDictionary;
    public event EventHandler<Type> InjectionChanged;
    public WeakEvent<Type> WeakInjectionChanged;
    private bool isUserDefined = false;
    public DIServiceCollection()
    {
        _serviceDictionary = new Dictionary<Type, DIServiceDescriptor>();
        WeakInjectionChanged = new WeakEvent<Type>();
        Register(typeof(ILogger), typeof(GameLogger), DILifeTime.Singleton);
        _container = new DiContainer(_serviceDictionary);
    }

    public DIServiceCollection(ILogger logger)
    {
        _serviceDictionary = new Dictionary<Type, DIServiceDescriptor>();
        WeakInjectionChanged = new WeakEvent<Type>();
        Register(typeof(ILogger), logger.GetType(), logger, DILifeTime.Singleton);
        _container = new DiContainer(_serviceDictionary);
    }
    public DIServiceCollection AddSingleton<T>()
    {
        Type type = typeof(T);
        Register(type, type, DILifeTime.Singleton);
        return this;
    }

    public DIServiceCollection AddSingleton<T>(T implementation)
    {
        Type type = typeof(T);
        Register(type, implementation.GetType(), implementation, DILifeTime.Singleton);
        return this;
    }
    public DIServiceCollection AddSingleton(DIServiceDescriptor descriptor)
    {
        Register(descriptor.ImplementationType, typeof(object), descriptor.Implementation, DILifeTime.Singleton);
        return this;
    }
    public DIServiceCollection AddSingleton<TSource>(DIServiceDescriptor descriptor)
    {
        Register(typeof(TSource), descriptor.ImplementationType, descriptor.Implementation, DILifeTime.Singleton);
        return this;
    }

    public DIServiceCollection AddSingleton<TSource, TImplementation>() where TImplementation: TSource
    {
        Type typeSource = typeof(TSource);
        Type implementationType = typeof(TImplementation);
        Register(typeSource, implementationType, DILifeTime.Singleton);
        return this;
    }
    /*public void RegisterSingleton<TSource, TImplementation>(TImplementation implementation) where TImplementation: TSource
    {
        Type typeSource = typeof(TSource);
        Type implementationType = typeof(TImplementation);
        Register(typeSource, implementationType,implementation ,DILifeTime.Singleton);
    }*/
    public DIServiceCollection AddTransient<T>()
    {
        Type type = typeof(T);
        Register(type, type, DILifeTime.Transient);
        return this;
    }

    public DIServiceCollection AddTransient(DIServiceDescriptor descriptor)
    {
        Register(descriptor.ImplementationType, typeof(object), descriptor.Implementation, DILifeTime.Transient);
        return this;
    }
    public DIServiceCollection AddTransient<TSource>(DIServiceDescriptor descriptor)
    {
        Register(typeof(TSource), descriptor.ImplementationType, descriptor.Implementation, DILifeTime.Transient);
        return this;
    }
    public DIServiceCollection AddTransient<TSource, TImplementation>() where TImplementation: TSource
    {
        Type typeSource = typeof(TSource);
        Type implementationType = typeof(TImplementation);
        Register(typeSource, implementationType, DILifeTime.Transient);
        return this;
    }
    public DIServiceCollection AddScoped<T>()
    {
        Type type = typeof(T);
        Register(type, type, DILifeTime.Scoped);
        return this;
    }
    public DIServiceCollection AddScoped(DIServiceDescriptor descriptor)
    {
        Register(descriptor.ImplementationType, typeof(object), descriptor.Implementation, DILifeTime.Scoped);
        return this;
    }
    public DIServiceCollection AddScoped<TSource>(DIServiceDescriptor descriptor)
    {
        Register(typeof(TSource), descriptor.ImplementationType, descriptor.Implementation, DILifeTime.Scoped);
        return this;
    }

    public DIServiceCollection AddScoped<TSource, TImplementation>() where TImplementation: TSource
    {
        Type typeSource = typeof(TSource);
        Type implementationType = typeof(TImplementation);
        Register(typeSource, implementationType, DILifeTime.Scoped);
        return this;
    }

    public DIServiceCollection AddBackgroundService<T>() where T : BackgroundService
    {
        AddSingleton<T>();
        return this;
    }

    private void Register(Type sourceType, Type implementationType , DILifeTime lifeTime)
    {
        if(_serviceDictionary.ContainsKey(sourceType))
        {
            _serviceDictionary[sourceType] = new DIServiceDescriptor(implementationType, lifeTime);
            InjectionChanged?.Invoke(null, sourceType);
            WeakInjectionChanged?.Invoke(null, sourceType);
            return;
            //throw new ArgumentException($"Service with the {sourceType} source type has already been registered");
        }
        _serviceDictionary.Add(sourceType, new DIServiceDescriptor(implementationType, lifeTime));
    }

    private void Register(Type sourceType, Type implementationType , object implementation, DILifeTime lifeTime)
    {
        if(_serviceDictionary.ContainsKey(sourceType))
        {
            _serviceDictionary[sourceType] = new DIServiceDescriptor(implementationType, lifeTime);
            InjectionChanged?.Invoke(null, sourceType);
            WeakInjectionChanged?.Invoke(null, sourceType);
            return;
            //throw new ArgumentException($"Service with the {sourceType} source type has already been registered");
        }
        _serviceDictionary.Add(sourceType, new DIServiceDescriptor(implementationType, implementation, lifeTime));
    }
    
    public DiContainer BuildServiceProvider()
    {
        return _container! ?? throw new UnreachableException("DIContainer doesn't exist");
    }

    internal void TryAdd(IEnumerable<DIServiceDescriptor> serviceDescriptor)
    {
        foreach (DIServiceDescriptor diServiceDescriptor in serviceDescriptor)
        {
            Type sourceType = diServiceDescriptor.ImplementationType;
            _serviceDictionary.Add(sourceType, diServiceDescriptor);
        }
    }

    private DiContainer _container;
    public void CreateScope()
    {
        if (!isUserDefined)
        {
            TryEndScope();
            isUserDefined = true;
        }
        _container.currentScope = Guid.NewGuid().ToString();
    }
    
    public bool TryCreateScope()
    {
        if (isUserDefined)
        {
            return false;
        }
        _container.currentScope = Guid.NewGuid().ToString();
        return true;
    }

    public void EndScope()
    {
        if (!isUserDefined)
        {
            throw new Exception("Trying to end unmanaged scope");
        }

        isUserDefined = false;
        _container.currentScope = null;
    }

    public bool TryEndScope()
    {
        if (isUserDefined)
        {
            return false;
        }
        _container.currentScope = null;
        return true;
    }
}