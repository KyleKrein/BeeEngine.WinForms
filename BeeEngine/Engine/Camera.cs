using BeeEngine.Drawing;
using BeeEngine.Vector;

namespace BeeEngine;

[DebuggerDisplay("X: {Position.X}, Y: {Position.Y}, Scale: {Scale}")]
public static class Camera
{
    public static Vector2 Position { get; internal set; } = Vector2.Zero();
    private static Vector2 _newPosition = Position;
    public static float MaxZoomIn { get; private set; } = -1;
    public static float MaxZoomOut { get; private set; } = -1;
    public static float Scale { get; private set; } = 1.0f;

    private static Matrix _zoomMatrix = new Matrix(1, 0,
                                                  0, 1,
                                                  0, 0);

    internal static float CameraAngle = 0;
    public static float OrthographicSize { get; set; } = 10;

    public static Vector4 Display
    {
        get
        {
            if (GameApplication.Instance is null)
                return default(Vector4);
            return new Vector4(-CenterPoint.X / Scale + Position.X, -CenterPoint.Y / Scale + Position.Y, GameApplication.Instance.Window.ClientSize.Width / Scale, GameApplication.Instance.Window.ClientSize.Height / Scale);

            //return new Vector4(CenterPoint.X / Scale + Position.X, CenterPoint.Y / Scale + Position.Y, GameApplication.Instance.Window.ClientSize.Width / Scale, GameApplication.Instance.Window.ClientSize.Height / Scale);
        }
    }

    private static float DefaultSpeed = 1;

    public static bool UseFixedUpdate = true;

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

    public static event CameraMoved StateChange;

    public static WeakEvent<CameraEventArgs> WeakStateChange = new WeakEvent<CameraEventArgs>();

    private static Vector2? startPosition;
    private static Vector2? endPosition;

    private static Vector4? GameWorld;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StateChanges()
    {
        var eventArgs = new CameraEventArgs(Position, Scale);
        if (StateChange != null)
            StateChange(eventArgs);
        WeakStateChange.Invoke(null, eventArgs);
    }

    public static bool SetScale(float scale)
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

    public static bool ChangeScale(float deltaScale)
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

    public static void SetNewPosition(float x, float y)
    {
        SetNewPosition(new Vector2(x, y));
    }

    public static void SetNewPosition(Vector2 NewPosition)
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
    private static bool isScaleCorrect(float newScale)
    {
        return newScale > 0 && (MaxZoomOut == -1 || newScale >= MaxZoomOut) && (MaxZoomIn == -1 || newScale <= MaxZoomIn);
    }

    public static void SetMaxZoomIn(float maxZoomIn)
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

    public static void SetMaxZoomOut(float maxZoomOut)
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
    public static void SetDefaultSpeed(float speed)
    {
        DefaultSpeed = speed;
    }

    public static void SetGameWorldCorners(Vector4 WorldSize)
    {
        GameWorld = WorldSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Up()
    {
        return Up(DefaultSpeed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Down()
    {
        return Down(DefaultSpeed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Left()
    {
        return Left(DefaultSpeed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Right()
    {
        return Right(DefaultSpeed);
    }

    public static float Up(float speed)
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

    public static float Down(float speed)
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

    public static float Left(float speed)
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

    public static float Right(float speed)
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

    private static void StopGOTO()
    {
        if (startPosition != null && endPosition != null)
        {
            startPosition = null;
            endPosition = null;
            _newPosition = Position;
        }
    }

    public static void TeleportTo(Vector2 coords)
    {
        if (IsInsideWorldBorders(coords))
        {
            Position = coords;
            StateChanges();
        }
    }

    public static void TeleportTo(float x, float y)
    {
        TeleportTo(new Vector2(x, y));
    }

    public static float movementTime = 3f;

    internal static void GoToWORK()
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

    private static bool delta(Vector2 a, Vector2 b, float d)
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
    public static void GoTo(Vector2 Destination)
    {
        if (Position != Destination && IsInsideWorldBorders(Destination))
        {
            startPosition = Position;
            endPosition = Destination;
            //CurrentSpeed = (float) (DefaultSpeed / (startPosition*endPosition));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GoTo(float x, float y)
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

    private static bool IsInsideWorldBorders(Vector2 newCameraPosition)
    {
        if (GameWorld == null)
        {
            return true;
        }
        //return Collision.IsColliding2D(GameWorld.Value*2, Display);
        return true;
    }

    static Camera()
    {
        if (GameApplication.Instance is null)
            throw new InvalidOperationException($"{nameof(GameApplication.Instance)} can't be null when using Camera");
        CenterPoint = new Vector2(GameApplication.Instance.Window.ClientSize.Width / 2, GameApplication.Instance.Window.ClientSize.Height / 2);
        GameApplication.Instance.Window.ClientSizeChanged += Window_ClientSizeChanged;
    }

    private static void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        CenterPoint = new Vector2(GameApplication.Instance!.Window.ClientSize.Width / 2, GameApplication.Instance!.Window.ClientSize.Height / 2);
    }

    internal static Vector2 CenterPoint;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Move(Graphics g)
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

    internal static void Move(FastGraphics g)
    {
        g.ResetTransform();
        g.TransformTranslate(CenterPoint);
        g.ScaleTransform(Scale);
        g.TransformTranslate(Position);
    }
}