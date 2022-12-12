using BeeEngine.Tasks;

namespace BeeEngine;

public abstract class GameEngine
{
    public WinForm Window { get; private set; }
    internal readonly RenderingQueue RenderingQueue;

    //private readonly Thread GameLoopThread;
    //private readonly Thread FixedUpdateThread;

    /*public static CompositingQuality UseCompositingQuality = CompositingQuality.Default;
    public static InterpolationMode UseInterpolationMode = InterpolationMode.NearestNeighbor;
    public static SmoothingMode UseSmoothingMode = SmoothingMode.HighQuality;
    public static PixelOffsetMode UsePixelOffsetMode = PixelOffsetMode.Half;*/

    private readonly WeakCollection<GameObject> AllGameObjects = new WeakCollection<GameObject>();
    private readonly WeakCollection<GameObject> UpdateGameObjects = new WeakCollection<GameObject>();
    private readonly WeakCollection<GameObject> LateUpdateGameObjects = new WeakCollection<GameObject>();
    private readonly WeakCollection<GameObject> FixedUpdateGameObjects = new WeakCollection<GameObject>();
    private readonly List<GameObject> GameObjectsWaitingForInit = new List<GameObject>();

    //private readonly CancellationTokenSource _cancellationGameLoop;
    //private readonly CancellationTokenSource _cancellationFixedGameLoop;

    private int _fpsLimit = 60;
    private int _limit = 16;

    internal Color BackgroundColor = Color.White;
    
    //protected bool StartInitializingGameObjects = false;
    private int FPSLimit
    {
        get
        {
            return _fpsLimit;
        }
        set
        {
            if (value <= 1000)
            {
                _fpsLimit = value;
                _limit = 1000 / _fpsLimit;
                Log.Debug($"FPS Limit: {_fpsLimit}\nDelay = {_limit}");
            }
            else
            {
                throw new Exception("FPS Limit is beyond 1000");
            }
        }
    }

    private bool _onLoadFired = false;

    private BackgroundTask _mainGameLoopThread;
    private BackgroundTask _fixedGameLoopThread;


    protected GameEngine()
    {
        RenderingQueue = new RenderingQueue(this);
    }
    public void Start(Vector2 screenSize, string windowTitle)
    {
        if (GameApplication.Instance!=null)
            throw new Exception("Can't start two instances of the game at the same time");
        GameApplication.Instance = this;
        Window = new WinForm(RenderingQueue);
        Window.Size = new Size((int)screenSize.X, (int)screenSize.Y);
        Window.Text = windowTitle;
        //Window.Paint += Renderer;
        Window.KeyPreview = true;
        Window.KeyUp += Window_KeyUp;
        Window.KeyDown += Window_KeyDown;
        Window.FormClosing += Window_FormClosing;
        Window.MouseClick += Window_MouseClick;
        Window.MouseWheel += Window_MouseWheel;
        //GameLoopThread.Start();
        if(!_onLoadFired)
            Load();
        _mainGameLoopThread = new BackgroundTask(TimeSpan.FromMilliseconds(1), OneFrameMethod);//new AsyncThread(OneFrameMethod).Repeat(TimeSpan.FromMilliseconds(1)); /*BackgroundTask(TimeSpan.FromMilliseconds(1), OneFrameMethod);*/
        _fixedGameLoopThread = new BackgroundTask(TimeSpan.FromMilliseconds((int) Time._fixedDeltaTime), FixedOneFrame);//new AsyncThread(FixedOneFrame).Repeat(TimeSpan.FromMilliseconds((int)Time._fixedDeltaTime));//new BackgroundTask(TimeSpan.FromMilliseconds((int)Time._fixedDeltaTime), FixedOneFrame);
        _mainGameLoopThread.Start();
        _fixedGameLoopThread.Start();
        //FixedUpdateThread.Start();
        Application.Run(Window);
    }

    public void Start()
    {
        Start(new Vector2(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height), "Game");
    }

    public void Start(string windowTitle)
    {
        Start(new Vector2(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), windowTitle);
    }
    /// <summary>
    /// Invokes OnLoad method if it wasn't Invoked before
    /// </summary>
    public void Load()
    {
        InitGameSettings();
        if (_onLoadFired)
            throw new Exception("Trying to call OnLoad after it already fired");
        _onLoadFired = true;
        OnLoad();
    }

    public async Task StopAsync()
    {
        if (!_onLoadFired)
            throw new Exception("Trying to stop Game before starting");
        GameCloses?.Invoke(this, EventArgs.Empty);
        WeakGameCloses.Invoke(this, EventArgs.Empty);
        //_cancellationGameLoop.Cancel();
        await _mainGameLoopThread.StopAsync();
        await _fixedGameLoopThread.StopAsync();
        //_cancellationFixedGameLoop.Cancel();
        //GameLoopThread.AbortCore();
        //FixedUpdateThread.AbortCore();
        /*while (GameLoopThread.IsAlive || FixedUpdateThread.IsAlive)
        {
            
        }*/
        RenderingQueue.Dispose();
        
        Window.Close();
        GameApplication.Instance = null;
        /*Application.Exit();
        Process.GetCurrentProcess().Kill();*/
    }

    public event EventHandler<EventArgs> GameCloses;
    private WeakEvent<EventArgs> _weakGameCloses = new WeakEvent<EventArgs>();
    public WeakEvent<EventArgs> WeakGameCloses
    {
        get => _weakGameCloses;
        set
        {
            if (_weakGameCloses.EventID != value.EventID)
                throw new ArgumentException("Can't assign weak event with different ID");
            _weakGameCloses = value;
        }
    }

    /// <summary>
    /// Конструктор, открывающий окно и запускающий рендер кадров
    /// </summary>
    

    private void Window_MouseWheel(object sender, MouseEventArgs e)
    {
        GetMouseWheel(e);
    }

    private void Window_MouseClick(object sender, MouseEventArgs e)
    {
        GetMouseClick(e);
    }

    private async void Window_FormClosing(object sender, FormClosingEventArgs e)
    {
        await WindowClosing();
    }

    protected virtual async Task WindowClosing()
    {
        //GameLoopThread.Abort();
        //GameLoopThread = null;
        /*foreach (var item in AllGameObjects.ToArray())
        {
            item.Dispose();
        }*/
        await StopAsync();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        GetKeyDown(e);
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        GetKeyUp(e);
    }

    /*internal static void AddSprite(Sprite2D sprite)
    {
        Task.Run(() =>
        {
            while (SpritesAreUpdating)
            {
                Thread.Sleep(0);
            }
            AllSprites2D.Add(sprite);
        });
    }*/

    /// <summary>
    /// Удаляет Спрайт из списка
    /// </summary>
    /*internal static void RemoveSprite(Sprite2D sprite)
    {
        Task.Run(() =>
        {
            while (SpritesAreUpdating)
            {
                Thread.Sleep(0);
            }
            AllSprites2D.Remove(sprite);
        });
        //AllSprites2D.Remove(sprite);
    }*/

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddGameObject(GameObject gameObject)
    {
        GameApplication.Instance?.AllGameObjects.Add(gameObject);
        GameApplication.Instance?.GameObjectsWaitingForInit.Add(gameObject);
        /*AllGameObjects.Add(gm);
        GameObjectsWaitingForInit.Add(gm);*/
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RemoveGameObject(GameObject gameObject)
    {
        GameApplication.Instance?.AllGameObjects.Remove(gameObject);
        GameApplication.Instance?.GameObjectsWaitingForInit.Remove(gameObject);
        GameApplication.Instance?.UpdateGameObjects.Remove(gameObject);
        GameApplication.Instance?.LateUpdateGameObjects.Remove(gameObject);
        GameApplication.Instance?.FixedUpdateGameObjects.Remove(gameObject);
    }
    public static void AddGameObject(GameObject gameObject, GameEngine gameInstance)
    {
        GameApplication.Instance?.AllGameObjects.Add(gameObject);
        GameApplication.Instance?.GameObjectsWaitingForInit.Add(gameObject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveGameObject(GameObject gameObject, GameEngine gameInstance)
    {
        if (gameInstance is null)
            throw new ArgumentNullException($"{nameof(gameInstance)} can't be null");
        if (gameObject is null)
            throw new ArgumentNullException($"{nameof(gameObject)} can't be null");
        gameInstance.AllGameObjects.Remove(gameObject);
        gameInstance.GameObjectsWaitingForInit.Remove(gameObject);
        gameInstance.UpdateGameObjects.Remove(gameObject);
        gameInstance.LateUpdateGameObjects.Remove(gameObject);
        gameInstance.FixedUpdateGameObjects.Remove(gameObject);
    }

    /// <summary>
    /// Отвечает за цикл обновления кадров: рендерит кадр с OnDraw(), затем обновляет кадр на экране и выолняет действия после обновления кадра (также устанавливается задержка)
    /// </summary>
    /*private void GameLoop()
    {
        if(!_onLoadFired)
            Load();
        //Stopwatch timer = new Stopwatch();
        //timer.Start();
        while (!_cancellationGameLoop.IsCancellationRequested)
        {
            OneFrameMethod();
            Thread.Sleep(0);
        }
        Log.Debug("GameLoop finished");
        //throw new Exception("GameLoop finished");
    }*/

    private void OneFrameMethod()
    {
        //Global.DependencyInjector.TryCreateScope();
        OnDraw();
        InitGameObjects();
        Update();
        LateUpdate();
        //Log.Info($"GameObjects: {AllGameObjects.Count}");
        Window.Invoke((MethodInvoker) delegate
        {
            //Window.ResumeLayout();
            Window.Invalidate();
            //Window.SuspendLayout();
        });
    }

    /*private void FixedUpdateLoop()
    {
        int D = (int)Time._fixedDeltaTime;
        while (!_cancellationFixedGameLoop.IsCancellationRequested)
        {
            FixedOneFrame();
            Thread.Sleep(D);
        }
        Log.Debug("FixedGameLoop finished");
        //throw new Exception("FixedGameLoop finished");
        return;
    }*/

    private void FixedOneFrame()
    {
        foreach (var gameObject in FixedUpdateGameObjects)
        {
            if (!gameObject.Enabled) continue;
            if (!gameObject.Invoke("FixedUpdate"))
            {
                FixedUpdateGameObjects.Remove(gameObject);
            }
        }

        OnFixedUpdate();
    }

    private void Update()
    {
        OnUpdate();
        foreach (var gameObject in UpdateGameObjects)
        {
            if (!gameObject.Enabled) continue;
            if (!gameObject.Invoke("Update"))
            {
                UpdateGameObjects.Remove(gameObject);
            }
        }
    }

    private void LateUpdate()
    {
        foreach (var gameObject in LateUpdateGameObjects)
        {
            if (!gameObject.Enabled) continue;
            if (!gameObject.Invoke("LateUpdate"))
            {
                LateUpdateGameObjects.Remove(gameObject);
            }
        }
    }

    private void InitGameObjects()
    {
        for (int i = 0; i < GameObjectsWaitingForInit.Count; i++/*.ToArray()*/)
        {
            GameObjectsWaitingForInit[i].Invoke("Start");

            UpdateGameObjects.Add(GameObjectsWaitingForInit[i]);
            LateUpdateGameObjects.Add(GameObjectsWaitingForInit[i]);
            FixedUpdateGameObjects.Add(GameObjectsWaitingForInit[i]);
            Log.Debug($"Игровой объект {GameObjectsWaitingForInit[i].Tag}: {GameObjectsWaitingForInit[i].Name} создан");
        }
        GameObjectsWaitingForInit.Clear();
    }

    internal IGameSettings _gameSettings;
    protected IGameSettings GameSettings
    {
        get => _gameSettings;
        private set => _gameSettings = value;
    }

    protected void RegisterGameSettings(IGameSettings gameSettings)
    {
        if (GameSettings is not null)
        {
            gameSettings.WeakChanged -= (sender,e)=>GameSettingsOnChanged(sender,e);
        }
        GameSettings = gameSettings;
        GameSettings.WindowHeight = new GameWindowSize(true, this);
        GameSettings.WindowWidth = new GameWindowSize(false, this);
        GameSettings.WeakChanged += (sender,e)=>GameSettingsOnChanged(sender,e);
    }

    private void GameSettingsOnChanged(object? sender, EventArgs e)
    {
        FPSLimit = GameSettings!.Fps;
        BackgroundColor = GameSettings.BackgroundColor;
    }

    

    private void InitGameSettings()
    {
        if(GameSettings != null)
            return;
        RegisterGameSettings(new GameSettings());
    }

    //абстрактные методы, которые используются потом для создания игры и игрового мира
    /// <summary>
    /// This happens once before the first frame after startup
    /// </summary>
    protected abstract void OnLoad();
    /// <summary>
    /// This is being called every frame. Should be used for game logic
    /// </summary>
    protected abstract void OnUpdate();
    /// <summary>
    /// This is being called every frame. Should be used for drawing
    /// </summary>
    protected abstract void OnDraw();
    /// <summary>
    /// This is being called 50 times per second. Use it for physics or to make something not rely on FPS
    /// </summary>
    protected abstract void OnFixedUpdate();

    protected abstract void GetKeyDown(KeyEventArgs e);

    protected abstract void GetKeyUp(KeyEventArgs e);

    protected abstract void GetMouseClick(MouseEventArgs e);

    protected abstract void GetMouseWheel(MouseEventArgs e);
}