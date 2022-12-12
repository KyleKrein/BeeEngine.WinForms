using BeeEngine.Vector;

namespace BeeEngine
{
    public static class Input
    {
        const short VK_LBUTTON = 0x01;
        const short VK_RBUTTON = 0x02;
        const short VK_MBUTTON = 0x03;
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(short vKey);

        public static int MouseX { get; internal set; }
        public static int MouseY { get; internal set; }

        public enum MouseButton
        {
            Left = VK_LBUTTON,
            Middle = VK_MBUTTON,
            Right = VK_RBUTTON
        }

        public static bool GetMouseButton(MouseButton mouseButton)
        {
            return GetAsyncKeyState((short)mouseButton) != 0;
        }

        public static bool GetMouseButtonDown(MouseButton mouseButton)
        {
            return GetAsyncKeyState((short)mouseButton) == -32767;
        }

        public static bool GetKeyDown(Keys key)
        {
            return GetAsyncKeyState((short)key) == -32767;
        }
        public static bool GetKeyDown(short vKey)
        {
            return GetAsyncKeyState(vKey) == -32767;
        }
        public static bool GetKey(Keys key)
        {
            return GetAsyncKeyState((short)key) != 0;
        }
        public static bool GetKey(short vKey)
        {
            return GetAsyncKeyState(vKey) != 0;
        }

        public static Vector2 GetMousePosition()
        {
            return new Vector2(MouseX, MouseY);
        }

        /*public static Vector2 GetMouseWorldPosition()
        {
            if (GameApplication.Instance is null)
                throw new NullReferenceException($"{nameof(GameApplication.Instance)} must be running");
            return new Vector2(MathU.Lerp(-Camera.OrthographicSize / 2, Camera.OrthographicSize / 2, MouseX / (float)GameApplication.Instance.Window.Width), MathU.Lerp(-Camera.OrthographicSize / 2, Camera.OrthographicSize / 2, MouseY / (float)GameApplication.Instance.Window.Height));
        }*/
    }
}
