namespace BeeEngine.Engine
{
    public static class SceneManager
    {
        public static event EventHandler<SceneLoadingEventArgs> SceneLoaded;

        public static event EventHandler<SceneLoadingEventArgs> SceneUnloaded;

        public static WeakEvent<SceneLoadingEventArgs> WeakSceneLoaded { get; set; }
        public static WeakEvent<SceneLoadingEventArgs> WeakSceneUnloaded { get; set; }

        public static bool IsLoaded { get; private set; }
        public static Scene CurrentScene { get; private set; }

        public static bool LoadScene(Scene scene)
        {
            if (scene == null)
            {
                return false;
            }
            UnloadScene();
            scene.Load();
            SceneLoadingEventArgs eventArgs = new SceneLoadingEventArgs(scene);
            SceneLoaded.Invoke(scene, eventArgs);
            WeakSceneLoaded?.Invoke(scene, eventArgs);
            return true;
        }

        public static bool UnloadScene()
        {
            if (IsLoaded)
            {
                CurrentScene.Unload();
                SceneLoadingEventArgs eventArgs = new SceneLoadingEventArgs(CurrentScene);
                SceneUnloaded?.Invoke(CurrentScene, eventArgs);
                WeakSceneUnloaded?.Invoke(CurrentScene, eventArgs);
                IsLoaded = false;
                return true;
            }
            return false;
        }

        public static async Task<bool> LoadSceneAsync(Scene scene)
        {
            if (scene == null)
            {
                return false;
            }
            var unloading = UnloadSceneAsync();
            var loading = scene.LoadAsync();
            unloading.Start();
            loading.Start();
            await unloading;
            await loading;
            SceneLoadingEventArgs eventArgs = new SceneLoadingEventArgs(scene);
            SceneLoaded.Invoke(scene, eventArgs);
            WeakSceneLoaded?.Invoke(scene, eventArgs);
            return true;
        }

        public static async Task<bool> UnloadSceneAsync()
        {
            if (IsLoaded)
            {
                await CurrentScene.UnloadAsync();
                SceneLoadingEventArgs eventArgs = new SceneLoadingEventArgs(CurrentScene);
                SceneUnloaded?.Invoke(CurrentScene, eventArgs);
                WeakSceneUnloaded?.Invoke(CurrentScene, eventArgs);
                IsLoaded = false;
                return true;
            }
            return false;
        }
    }

    public class SceneLoadingEventArgs
    {
        public Scene Scene { get; private set; }

        public SceneLoadingEventArgs(Scene scene)
        { this.Scene = scene; }
    }
}