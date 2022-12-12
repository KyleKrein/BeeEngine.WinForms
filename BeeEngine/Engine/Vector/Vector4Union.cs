namespace BeeEngine.Vector
{
    [StructLayout(LayoutKind.Explicit)]
    internal class Vector4Union
    {
        [FieldOffset(0)]
        private Vector2[] temp = new Vector2[2];

        //[System.Runtime.InteropServices.FieldOffset(0)]
        public Vector2 Position
        {
            get
            {
                return temp[0];
            }
            set
            {
                temp[0] = value;
            }
        }
        public Vector2 Scale
        {
            get
            {
                return temp[1];
            }
            set
            {
                temp[1] = value;
            }
        }

        [FieldOffset(0)]
        public Vector4 Vector4;

        public Vector4Union(float x, float y, float z, float w)
        {
            temp[0] = Vector2.Zero();
            temp[1] = Vector2.Zero();
            Vector4 = Vector4.Zero();
            Vector4.X = x;
            Vector4.Y = y;
            Vector4.Z = z;
            Vector4.W = w;
        }
    }
}
