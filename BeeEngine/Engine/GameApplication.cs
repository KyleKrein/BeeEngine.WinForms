using BeeEngine.DependencyInjection;

namespace BeeEngine
{
    public sealed class GameApplication
    {
        private static GameEngine? _instance;

        public static GameEngine? Instance
        {
            get
            {
                return _instance;
            }
            internal set
            {
                _instance = value;
            }
        }
        public DIServiceCollection Services { get; private set; }
        
        public GameBuilder CreateBuilder()
        {
            return new GameBuilder();
        }

        public GameEngine GetCurrentInstance()
        {
            if (_instance is null)
                throw new InvalidOperationException("There's no active instance");
            return _instance;
        }

        public GameApplication()
        {
            Services = new DIServiceCollection();
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}