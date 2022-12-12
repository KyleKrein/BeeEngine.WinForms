namespace BeeEngine.Vector
{
    public struct Vector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Vector4 Zero()
        {
            return new Vector4(0, 0, 0, 0);
        }

        public static Vector4 One()
        {
            return new Vector4(1, 1, 1, 1);
        }

        public static bool operator ==(Vector4? a, Vector4? b)
        {
            return a?.X == b?.X && a?.Y == b?.Y && a?.Y == b?.Y && a?.Y == b?.Y;
        }

        public static bool operator !=(Vector4? a, Vector4? b)
        {
            return !(a == b);
        }

        public static Vector4 operator /(Vector4 a, float b)
        {
            return new Vector4(a.X / b, a.Y / b, a.W / b, a.Z / b);
        }

        public static Vector4 operator *(Vector4 a, float b)
        {
            return new Vector4(a.X * b, a.Y * b, a.W * b, a.Z * b);
        }

        private const string xStart = "X = ";
        private const string xyMiddle = ", Y = ";
        private const string zStart = ", Z = ";
        private const string wStart = ", W = ";

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(xStart);
            stringBuilder.Append(X);
            stringBuilder.Append(xyMiddle);
            stringBuilder.Append(Y);
            stringBuilder.Append(zStart);
            stringBuilder.Append(Z);
            stringBuilder.Append(wStart);
            stringBuilder.Append(W);
            return stringBuilder.ToString();
        }
    }
}