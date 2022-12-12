namespace BeeEngine.Vector
{
    public struct Vector4Int
    {
        public int X;
        public int Y;
        public int Z;
        public int W;

        public Vector4Int(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Vector4Int Zero()
        {
            return new Vector4Int(0, 0, 0, 0);
        }

        public static Vector4Int One()
        {
            return new Vector4Int(1, 1, 1, 1);
        }

        public static bool operator ==(Vector4Int? a, Vector4Int? b)
        {
            return a?.X == b?.X && a?.Y == b?.Y && a?.Y == b?.Y && a?.Y == b?.Y;
        }
        public static bool operator !=(Vector4Int? a, Vector4Int? b)
        {
            return !(a == b);
        }

        public static Vector4Int operator /(Vector4Int a, int b)
        {
            return new Vector4Int(a.X / b, a.Y / b, a.W / b, a.Z / b);
        }
        public static Vector4Int operator *(Vector4Int a, int b)
        {
            return new Vector4Int(a.X * b, a.Y * b, a.W * b, a.Z * b);
        }
    }
}
