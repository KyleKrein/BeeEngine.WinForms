namespace BeeEngine.Vector
{
    [Serializable]
    public struct Vector2Int
    {
        public int X;
        public int Y;

        public Vector2Int()
        {
            X = 0;
            Y = 0;
        }
        public static Vector2Int Lerp(Vector2Int firstVector, Vector2Int secondVector, float by)
        {
            int retX = NumberHelper.LerpInt(firstVector.X, secondVector.X, by);
            int retY = NumberHelper.LerpInt(firstVector.Y, secondVector.Y, by);
            return new Vector2Int(retX, retY);
        }
        public static Vector2Int Lerp(Vector2Int firstVector, Vector2Int secondVector, double by)
        {
            int retX = NumberHelper.LerpInt(firstVector.X, secondVector.X, by);
            int retY = NumberHelper.LerpInt(firstVector.Y, secondVector.Y, by);
            return new Vector2Int(retX, retY);
        }
        public Vector2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        /// <summary>
        /// Returns X,Y as 0
        /// </summary>
        /// <returns></returns>
        public static Vector2Int Zero()
        {
            return new Vector2Int(0, 0);
        }
        public static Vector2Int One()
        {
            return new Vector2Int(1, 1);
        }

        public static bool operator ==(Vector2Int? a, Vector2Int? b)
        {
            return a?.X == b?.X && a?.Y == b?.Y;
        }

        public static bool operator !=(Vector2Int? a, Vector2Int? b)
        {
            return !(a == b);
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X + b.X, a.Y + b.Y);
        }
        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X + b.X, a.Y + b.Y);
        }

        public static explicit operator Vector2Int(Vector2 v)
        {
            return new Vector2Int((int)v.X, (int)v.Y);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }

        public static Vector2Int Load(BinaryReader reader)
        {
            int X = reader.ReadInt32();
            int Y = reader.ReadInt32();
            return new Vector2Int(X, Y);
        }
    }
}