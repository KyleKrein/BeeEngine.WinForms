namespace BeeEngine.Vector
{
    [Serializable]
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2()
        {
            X = Zero().X;
            Y = Zero().Y;
        }

        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Returns X,Y as 0
        /// </summary>
        /// <returns></returns>
        public static Vector2 Zero()
        {
            return new Vector2(0, 0);
        }

        public static Vector2 One()
        {
            return new Vector2(1, 1);
        }

        public static Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
        {
            float retX = NumberHelper.Lerp(firstVector.X, secondVector.X, by);
            float retY = NumberHelper.Lerp(firstVector.Y, secondVector.Y, by);
            return new Vector2(retX, retY);
        }

        public static bool operator ==(Vector2? a, Vector2? b)
        {
            return a?.X == b?.X && a?.Y == b?.Y;
        }

        public static bool operator !=(Vector2? a, Vector2? b)
        {
            return !(a == b);
        }

        //Distance formula
        public static double operator *(Vector2 a, Vector2 b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, b.Y + a.Y);
        }

        /*public static explicit operator Vector2(Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }*/

        public void Save(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }

        public static Vector2 Load(BinaryReader reader)
        {
            float X = reader.ReadSingle();
            float Y = reader.ReadSingle();
            return new Vector2(X, Y);
        }

        private const string xStart = "X = ";
        private const string xyMiddle = ", Y = ";

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(xStart);
            stringBuilder.Append(X);
            stringBuilder.Append(xyMiddle);
            stringBuilder.Append(Y);
            return stringBuilder.ToString();
        }
    }
}