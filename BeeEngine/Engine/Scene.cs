using BeeEngine.Drawing;

namespace BeeEngine.Engine
{
    public sealed class Scene
    {
        private List<GameObject> _gameObjects = new List<GameObject>();
        private List<Sprite2D> _sprites = new List<Sprite2D>();
        private readonly object locker = new object();
        public void AddGameObject(GameObject gameObject)
        {
            if (gameObject is Scene)
            {
                throw new ArgumentException("Can't add one Scene to a another Scene");
            }

            lock (locker)
            {
                _gameObjects.Add(gameObject);
            }
        }
        internal void Load()
        {
        }

        internal void Unload()
        {
        }

        internal async Task LoadAsync()
        {
        }

        internal async Task UnloadAsync()
        {
        }

        private void Update()
        {
            //TODO: write updating method for all sprites and GameObjects
        }
    }
}