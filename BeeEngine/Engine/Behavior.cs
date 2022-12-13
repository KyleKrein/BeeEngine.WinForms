using BeeEngine.Drawing;
using BeeEngine.Vector;
using System.Text.Json;
namespace BeeEngine
{
    [Serializable]
    public abstract class Behavior: Component
    {
        public GameObject GameObject { get; internal set; }
        
        protected GameObject Instantiate(GameObject gameObject)
        {
            GameObject newGameObject = new GameObject(gameObject);
            newGameObject.Transform = new Transform(gameObject.Transform,newGameObject);
            GameObject.AddComponent(newGameObject);
            newGameObject.Script?.Invoke("Awake");
            return newGameObject;
        }
        protected GameObject Instantiate(GameObject gameObject,Transform parent)
        {
            GameObject newGameObject = new GameObject(gameObject);
            newGameObject.Transform = new Transform(parent, newGameObject);
            GameObject.AddComponent(newGameObject);
            newGameObject.Script?.Invoke("Awake");
            return newGameObject;
        }
        protected GameObject Instantiate(GameObject gameObject,Vector3 position)
        {
            GameObject newGameObject = new GameObject(gameObject);
            newGameObject.Transform = new Transform(newGameObject);
            newGameObject.Transform.Position = position;
            GameObject.AddComponent(newGameObject);
            newGameObject.Script?.Invoke("Awake");
            return newGameObject;
        }

        protected T? GetComponent<T>() where T : Component
        {
            return BeeEngine.GameObject.GetComponent<T>();
        }

        protected IEnumerable<T> GetComponents<T>() where T : Component
        {
            return BeeEngine.GameObject.GetComponents<T>();
        }
    }
}