using BeeEngine.Drawing;
using BeeEngine.Vector;

namespace BeeEngine
{
    public abstract class ScriptBehavior: Component
    {
        public GameObject GameObject { get;}
        protected void Instantiate<T>(Component component) where T: Component
        {
            GameObject.AddComponent(component);
            component.Invoke("Awake");
        }

        /*protected T Instantiate<T>() where T: Component, new()
        {
            T newComponent = new T();
            GameObject.AddComponent(newComponent);
            newComponent.Invoke("Awake");
            return newComponent;
        }*/
        protected T Instantiate<T>(Transform parent) where T: Component, new()
        {
            T newComponent = new T();
            GameObject.AddComponent(newComponent);
            newComponent.Invoke("Awake");
            return newComponent;
        }
        protected T Instantiate<T>(Vector3 position) where T: Component, new()
        {
            T newComponent = new T();
            GameObject.AddComponent(newComponent);
            newComponent.Invoke("Awake");
            return newComponent;
        }
        /*protected void Instantiate<T>(Component component, Transform parent) where T: Component
        {
            var copy
            GameObject.AddComponent(component);
            component.Invoke("Awake");
        }*/

        protected T Instantiate<T>() where T: Component, new()
        {
            T newComponent = new T();
            GameObject.AddComponent(newComponent);
            newComponent.Invoke("Awake");
            return newComponent;
        }
    }
}