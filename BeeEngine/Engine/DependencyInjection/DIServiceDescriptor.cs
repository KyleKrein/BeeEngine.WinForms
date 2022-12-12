namespace BeeEngine.DependencyInjection
{
    public sealed class DIServiceDescriptor
    {
        public Type ImplementationType { get; private set; }
        public object? Implementation { get; internal set; }
        internal DILifeTime LifeTime { get; private set; }
        internal string? scope = null;

        private DIServiceDescriptor()
        {
        
        }

        internal DIServiceDescriptor(Type implementationType, object? implementation, DILifeTime lifeTime)
        {
            Implementation = implementation;
            ImplementationType = implementationType;
            LifeTime = lifeTime;
        }

        public DIServiceDescriptor(Type implementationType, object? implementation)
        {
            Implementation = implementation;
            ImplementationType = implementationType;
        }

        public DIServiceDescriptor(Type implementationType, DILifeTime lifeTime)
        {
            ImplementationType = implementationType;
            Implementation = null;
            LifeTime = lifeTime;
        }
    }
}