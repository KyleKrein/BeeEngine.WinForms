namespace BeeEngine.Vector
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3()
        {
            X = Zero().X;
            Y = Zero().Y;
            Z = Zero().Z;
        }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Returns X,Y,Z as 0
        /// </summary>
        /// <returns></returns>
        public static Vector3 Zero()
        {
            return new Vector3(0, 0, 0);
        }

        public static Vector3 One()
        {
            return new Vector3(1, 1, 1);
        }

        public static bool operator ==(Vector3? a, Vector3? b)
        {
            return a?.X == b?.X && a?.Y == b?.Y && a?.Z == b?.Z;
        }

        public static bool operator !=(Vector3? a, Vector3? b)
        {
            return !(a == b);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        private const string xStart = "X = ";
        private const string xyMiddle = ", Y = ";
        private const string zStart = ", Z = ";

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(xStart);
            stringBuilder.Append(X);
            stringBuilder.Append(xyMiddle);
            stringBuilder.Append(Y);
            stringBuilder.Append(zStart);
            stringBuilder.Append(Z);
            return stringBuilder.ToString();
        }
    }
}