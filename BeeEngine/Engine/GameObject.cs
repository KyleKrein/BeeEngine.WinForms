using BeeEngine.Drawing;
using BeeEngine.DependencyInjection;
using BeeEngine.Tasks;

namespace BeeEngine;

/// <summary>
/// Base class for all Game Objects. Can draw itself on screen. Define Start, Update, LateUpdate, FixedUpdate,
/// OnEnable, OnDisable methods if needed
/// </summary>
[Serializable]
public abstract class GameObject : Component, ICloneable, IEquatable<GameObject>, IDisposable
{
    private bool disposedValue;

    public virtual string Name { get; set; } = "";
    public virtual string Tag { get; set; } = "";
    private static readonly IComponentCollection ComponentsCollection = new ComponentCollection();
    private readonly IComponentCollection _childComponents = new ComponentCollection();
    public bool Enabled { get; private set; } = true;
    internal bool AutoManage { get; private set; }

    protected GameObject()
    {
        AddGameObject();
    }

    protected GameObject(bool autoManageEnabled)
    {
        AutoManage = autoManageEnabled;
        AddGameObject();
    }

    protected GameObject(GameObject gameObject)
    {
        Name = (string)gameObject.Name.Clone();
        Tag = (string)gameObject.Tag.Clone();
        AddGameObject();
    }
    protected T? GetComponent<T>() where T : Component
    {
        return ComponentsCollection.GetComponent<T>();
    }

    protected IEnumerable<T> GetComponents<T>() where T : Component
    {
        return ComponentsCollection.GetComponents<T>();
    }

    internal static void AddComponent<T>(T component) where T : Component
    {
        ComponentsCollection.AddComponent(component);
    }

    internal static void RemoveComponent<T>(T component) where T : Component
    {
        ComponentsCollection.RemoveComponent(component);
    }
    protected T? GetChildComponent<T>() where T : Component
    {
        return _childComponents.GetComponent<T>();
    }

    protected IEnumerable<T> GetChildComponents<T>() where T : Component
    {
        return _childComponents.GetComponents<T>();
    }

    protected void AddChildComponent<T>(T component) where T : Component
    {
        _childComponents.AddComponent(component);
    }

    protected void RemoveChildComponent<T>(T component) where T : Component
    {
        _childComponents.RemoveComponent(component);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enable()
    {
        if (!Enabled)
        {
            this.Invoke("OnEnable");
            Enabled = true;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Disable()
    {
        if (Enabled)
        {
            this.Invoke("OnDisable");
            Enabled = false;
        }
    }
    /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void OnPaint(FastGraphics g)
    {
        Paint?.Invoke(this, g);
        WeakPaint.Invoke(this, g);
    }*/

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddGameObject()
    {
        if(AutoManage)
            GameEngine.AddGameObject(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveGameObject()
    {
        if(AutoManage)
            GameEngine.RemoveGameObject(this);
    }
    private const char twoDots = ':';
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(Tag);
        stringBuilder.Append(twoDots);
        stringBuilder.Append(Name);
        stringBuilder.Append(twoDots);
        stringBuilder.Append(Id);
        return stringBuilder.ToString();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object Clone()
    {
        return MemberwiseClone();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void OnDispose()
    {
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GameObject? other)
    {
        return Name == other?.Name && Tag == other?.Tag && Id == other?.Id;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                OnDispose();
            }
            RemoveGameObject();
            Log.Debug($"Игровой объект {Name} уничтожен");
            disposedValue = true;
        }
    }
    ~GameObject()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public event EventHandler<FastGraphics> Paint;

    public event EventHandler<MouseEventArgs> MouseMove;

    public event EventHandler<MouseEventArgs> MouseClick;

    public event EventHandler<MouseEventArgs> MouseDown;

    public event EventHandler<MouseEventArgs> MouseUp;

    public event EventHandler<MouseEventArgs> MouseEnter;

    public event EventHandler<MouseEventArgs> MouseLeave;
}