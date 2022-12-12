namespace BeeEngine.Vector
{
    public struct Vector3Int
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3Int()
        {
            X = Zero().X;
            Y = Zero().Y;
            Z = Zero().Z;
        }

        public Vector3Int(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        /// <summary>
        /// Returns X,Y,Z as 0
        /// </summary>
        /// <returns></returns>
        public static Vector3Int Zero()
        {
            return new Vector3Int(0, 0, 0);
        }
        public static Vector3Int One()
        {
            return new Vector3Int(1, 1, 1);
        }

        public static bool operator ==(Vector3Int? a, Vector3Int? b)
        {
            return a?.X == b?.X && a?.Y == b?.Y && a?.Z == b?.Z;
        }

        public static bool operator !=(Vector3Int? a, Vector3Int? b)
        {
            return !(a == b);
        }
    }
}
