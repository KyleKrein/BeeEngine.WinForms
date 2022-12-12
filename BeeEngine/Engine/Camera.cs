using BeeEngine;

namespace GameEngine2D;

[DebuggerDisplay("X: {Position.X}, Y: {Position.Y}, Scale: {Scale}")]
public class Camera
{
    public static List<Camera> Cameras = new List<Camera>();
    public Vector2 Position { get; internal set; } = Vector2.Zero();
    private Vector2 _newPosition = Vector2.Zero();
    public float MaxZoomIn { get; private set; } = -1;
    public float MaxZoomOut { get; private set; } = -1;
    public float Scale { get; private set; } = 1.0f;

    public Size Size
    {
        get => _size;
        set
        {
            _size = value;
            CenterPoint = new Vector2(value.Width / 2, value.Height / 2);
            StateChanges();
        }
    }
    Size _size = Size.Empty;

    private Matrix _zoomMatrix = new Matrix(1, 0,
                                                  0, 1,
                                                  0, 0);

    internal float CameraAngle = 0;
    public float OrthographicSize { get; set; } = 10;

    public Camera()
    {
        Cameras.Add(this);
    }

    public Camera(Vector2 position, Size size, float scale):this()
    {
        Position = position;
        _newPosition = Position;
        Scale = scale;
        Size = size;
    }

    public Camera(Control control):this()
    {
        control.Resize += ((sender, args) => { Size = control.Size;});
    }
    ~Camera()
    {
        Cameras.Remove(this);
    }

    public Vector4 Bounds
    {
        get
        {
            return new Vector4(-CenterPoint.X / Scale + Position.X, -CenterPoint.Y / Scale + Position.Y, Size.Width / Scale, Size.Height / Scale);

            //return new Vector4(CenterPoint.X / Scale + Position.X, CenterPoint.Y / Scale + Position.Y, GameApplication.Instance.Window.ClientSize.Width / Scale, GameApplication.Instance.Window.ClientSize.Height / Scale);
        }
    }

    private float DefaultSpeed = 1;

    public bool UseFixedUpdate = true;

    public class CameraEventArgs
    {
        public float X => Position.X;
        public float Y => Position.Y;
        public float Scale;
        public Vector2 Position;

        public CameraEventArgs(Vector2 pos, float scale)
        {
            Position = pos;
            Scale = scale;
        }
    }

    public delegate void CameraMoved(CameraEventArgs e);

    public event CameraMoved StateChange;

    public WeakEvent<CameraEventArgs> WeakStateChange = new WeakEvent<CameraEventArgs>();

    private Vector2? startPosition;
    private Vector2? endPosition;

    private Vector4? GameWorld;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StateChanges()
    {
        var eventArgs = new CameraEventArgs(Position, Scale);
        if (StateChange != null)
            StateChange(eventArgs);
        WeakStateChange.Invoke(null, eventArgs);
    }

    public bool SetScale(float scale)
    {
        //if (scale <= 0)
        //    throw new Exception("Camera Scale set to a number that equals or less then zero");
        /*if (isScaleCorrect(scale))
        {
            Log.Error("Trying to set incorrect scale number");
            return false;
        }*/
        Scale = scale;
        //Position = new Vector2((int) (Position.X * scale), (int) (Position.Y * scale));
        StateChanges();
        return true;
    }

    public bool ChangeScale(float deltaScale)
    {
        float s = Scale + deltaScale;
        if (isScaleCorrect(s))
        {
            Scale = s;
            _zoomMatrix = new Matrix(Scale, 0,
                                    0, Scale,
                                    0, 0);
            StateChanges();
            return true;
        }
        return false;
    }

    public void SetNewPosition(float x, float y)
    {
        SetNewPosition(new Vector2(x, y));
    }

    public void SetNewPosition(Vector2 NewPosition)
    {
        if (IsInsideWorldBorders(NewPosition))
        {
            if (startPosition != null)
                startPosition = null;
            if (endPosition != null)
                endPosition = null;
            _newPosition = NewPosition;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool isScaleCorrect(float newScale)
    {
        return newScale > 0 && (MaxZoomOut == -1 || newScale >= MaxZoomOut) && (MaxZoomIn == -1 || newScale <= MaxZoomIn);
    }

    public void SetMaxZoomIn(float maxZoomIn)
    {
        if (maxZoomIn == -1 || maxZoomIn > 0)
        {
            MaxZoomIn = maxZoomIn;
            if (Scale > maxZoomIn)
            {
                SetScale(maxZoomIn);
            }
        }
        else
        {
            throw new Exception("Trying to set Camera Max Scale to numbers other then > 0 or -1");
        }
    }

    public void SetMaxZoomOut(float maxZoomOut)
    {
        if (maxZoomOut == -1 || maxZoomOut > 0)
        {
            MaxZoomOut = maxZoomOut;
            if (Scale < maxZoomOut)
            {
                SetScale(maxZoomOut);
            }
        }
        else
        {
            throw new Exception("Trying to set Camera Min Scale to numbers other then > 0 or -1");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDefaultSpeed(float speed)
    {
        DefaultSpeed = speed;
    }

    public void SetGameWorldCorners(Vector4 WorldSize)
    {
        GameWorld = WorldSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Up()
    {
        return Up(DefaultSpeed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Down()
    {
        return Down(DefaultSpeed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Left()
    {
        return Left(DefaultSpeed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Right()
    {
        return Right(DefaultSpeed);
    }

    public float Up(float speed)
    {
        var temp = new Vector2(Position.X, Position.Y + speed);

        if (IsInsideWorldBorders(temp))
        {
            StopGOTO();
            _newPosition.Y += speed;
            return speed;
        }
        return 0;
    }

    public float Down(float speed)
    {
        var temp = new Vector2(Position.X, Position.Y - speed);

        if (IsInsideWorldBorders(temp))
        {
            StopGOTO();
            _newPosition.Y -= speed;
            return speed;
        }
        return 0;
    }

    public float Left(float speed)
    {
        var temp = new Vector2(Position.X + speed, Position.Y);
        if (IsInsideWorldBorders(temp))
        {
            StopGOTO();
            _newPosition.X += speed;
            return speed;
        }
        return 0;
    }

    public float Right(float speed)
    {
        var temp = new Vector2(Position.X - speed, Position.Y);
        if (IsInsideWorldBorders(temp))
        {
            StopGOTO();
            _newPosition.X -= speed;
            return speed;
        }

        return 0;
    }

    private void StopGOTO()
    {
        if (startPosition != null && endPosition != null)
        {
            startPosition = null;
            endPosition = null;
            _newPosition = Position;
        }
    }

    public void TeleportTo(Vector2 coords)
    {
        if (IsInsideWorldBorders(coords))
        {
            Position = coords;
            StateChanges();
        }
    }

    public void TeleportTo(float x, float y)
    {
        TeleportTo(new Vector2(x, y));
    }

    public float movementTime = 3f;

    internal void GoToWORK()
    {
        float by = (float)(Time.deltaTime * movementTime);
        if (startPosition != null && endPosition != null)
        {
            Position = Vector2.Lerp(Position, endPosition.Value, by);

            if (delta(Position, endPosition.Value, 5))
            {
                startPosition = null;
                endPosition = null;
                _newPosition = Position;
            }
            StateChanges();
            return;
        }
        Position = Vector2.Lerp(Position, _newPosition, by);
        StateChanges();
    }

    private bool delta(Vector2 a, Vector2 b, float d)
    {
        if (a.X < b.X + d &&
            a.X + d > b.X &&
            a.Y < b.Y + d &&
            a.Y + d > b.Y)
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GoTo(Vector2 Destination)
    {
        if (Position != Destination && IsInsideWorldBorders(Destination))
        {
            startPosition = Position;
            endPosition = Destination;
            //CurrentSpeed = (float) (DefaultSpeed / (startPosition*endPosition));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GoTo(float x, float y)
    {
        GoTo(new Vector2(x, y));
    }

    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInsideWorldBorders()
    {
        if(GameWorld == Vector4.Zero())
            return true;
        return Collision.IsColliding2D(GameWorld,
            new Vector4(Position.X, Position.Y, GameEngine.Window.Width, GameEngine.Window.Height));
    }*/

    private bool IsInsideWorldBorders(Vector2 newCameraPosition)
    {
        if (GameWorld == null)
        {
            return true;
        }
        //return Collision.IsColliding2D(GameWorld.Value*2, Display);
        return true;
    }

    /*static Camera()
    {
        if (GameApplication.Instance is null)
            throw new InvalidOperationException($"{nameof(GameApplication.Instance)} can't be null when using Camera");
        CenterPoint = new Vector2(GameApplication.Instance.Window.ClientSize.Width / 2, GameApplication.Instance.Window.ClientSize.Height / 2);
        GameApplication.Instance.Window.ClientSizeChanged += Window_ClientSizeChanged;
    }*/

    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        CenterPoint = new Vector2(GameApplication.Instance!.Window.ClientSize.Width / 2, GameApplication.Instance!.Window.ClientSize.Height / 2);
    }

    internal Vector2 CenterPoint;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Move(Graphics g)
    {
        Matrix matrix = new Matrix();
        matrix.Translate(CenterPoint.X, CenterPoint.Y);
        matrix.Scale(Scale, Scale);
        matrix.Translate(Position.X, Position.Y);
        g.ResetTransform();
        //g.TranslateTransform(CenterPoint.X,CenterPoint.Y);
        //zoomMatrix.Translate(CenterPoint.X,CenterPoint.Y);
        g.MultiplyTransform(matrix);
        //g.TranslateTransform(Position.X, Position.Y);
    }

    internal void Move(FastGraphics g)
    {
        g.ResetTransform();
        g.TransformTranslate(CenterPoint);
        g.ScaleTransform(Scale);
        g.TransformTranslate(Position);
    }

    public void DrawFrame(Graphics g)
    {
        Move(g);
        
        g.ResetTransform();
    }
    public void DrawFrame(FastGraphics g)
    {
        Move(g);
        
        g.ResetTransform();
    }
    public void DrawFrame(Bitmap targetBitmap)
    {
        
    }
}