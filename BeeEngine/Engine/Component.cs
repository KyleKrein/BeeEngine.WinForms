using System.Collections.Concurrent;
using System.Collections.Immutable;
using BeeEngine.Tasks;

namespace BeeEngine
{
    public abstract class Component
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task Invoke(string method, double time, params object?[] args)
        {
            FireOnceBackgroundTask task = new FireOnceBackgroundTask(TimeSpan.FromMilliseconds(time), () =>
            {
                StartInvoke(method, args);
            });
            await task.StartAsync();
        }
        public static MethodInfo? AwakeMethod = typeof(Component).GetMethod("Awake",
            BindingFlags.NonPublic | BindingFlags.Instance);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Invoke(string Method, params object?[] args)
        {
            if (Method == "Awake")
            {
                AwakeMethod?.Invoke(this, args);
                return true;
            }
            return StartInvoke(Method, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool StartInvoke(String methodName, params object?[] args)
        {
            MethodInfo? m = GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (m == null)
            {
                if (methodName != "Awake" && methodName != "Start" && methodName != "Update" && methodName != "LateUpdate" && methodName != "FixedUpdate")
                    Log.Error($"Can't invoke method {methodName} on Component {this.ToString()}");
                return false;
            }
            m.Invoke(this, args);
            return true;
        }
    }

    internal interface IComponentCollection
    {
        T? GetComponent<T>() where T: Component;
        IEnumerable<T> GetComponents<T>() where T: Component;
        void AddComponent<T>(T component) where T: Component;
        void RemoveComponent<T>(T component) where T : Component;
    }

    class ComponentCollection : IComponentCollection
    {
        private readonly ConcurrentDictionary<Type, List<ComponentInfo>> _componentDictionary = new ConcurrentDictionary<Type, List<ComponentInfo>>();
        public T? GetComponent<T>() where T: Component
        {
            var componentType = typeof(T);
            List<ComponentInfo> componentCollection;
            if (!_componentDictionary.TryGetValue(componentType, out componentCollection))
            {
                componentCollection = new List<ComponentInfo>();
                _componentDictionary[componentType] = componentCollection;
            }

            return (T)componentCollection.FirstOrDefault()?.Instance!;
        }

        public IEnumerable<T> GetComponents<T>() where T: Component
        {
            var componentType = typeof(T);
            List<ComponentInfo> componentCollection;
            if (!_componentDictionary.TryGetValue(componentType, out componentCollection))
            {
                componentCollection = new List<ComponentInfo>();
                _componentDictionary[componentType] = componentCollection;
            }

            return componentCollection.Select((x) => (T)x.Instance).ToImmutableList();
        }

        public void AddComponent<T>(T component) where T: Component
        {
            var componentType = typeof(T);
            List<ComponentInfo> componentCollection;
            if (!_componentDictionary.TryGetValue(componentType, out componentCollection))
            {
                componentCollection = new List<ComponentInfo>();
                _componentDictionary[componentType] = componentCollection;
            }

            componentCollection.Append(new ComponentInfo(component, componentType));
        }

        public void RemoveComponent<T>(T component) where T : Component
        {
            var componentType = typeof(T);
            _componentDictionary[componentType].Remove(component);
        }
    }

    internal class ComponentInfo
    {
        public Component Instance;
        public Type InstanceType;

        public ComponentInfo(Component instance, Type instanceType)
        {
            Instance = instance;
            InstanceType = instanceType;
        }

        public static implicit operator ComponentInfo(Component component)
        {
            return new ComponentInfo(component, component.GetType());
        }

        public static bool operator ==(ComponentInfo componentInfo, Component component)
        {
            return componentInfo.Instance.Id == component.Id;
        }

        public static bool operator !=(ComponentInfo componentInfo, Component component)
        {
            return !(componentInfo == component);
        }
    }
}
