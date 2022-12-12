using BeeEngine.Drawing;

namespace BeeEngine.Vector
{
    public static class Collision
    {
        public static bool IsColliding2D(Vector2 a_pos, Vector2 a_size, Vector2 b_pos, Vector2 b_size)
        {
            if (a_pos.X < b_pos.X + b_size.X &&
                a_pos.X + a_size.X > b_pos.X &&
                a_pos.Y < b_pos.Y + b_size.Y &&
                a_pos.Y + a_size.Y > b_pos.Y)
            {
                return true;
            }
            return false;
        }
        /*public static bool IsColliding2D(Vector2 a_pos, Vector2 a_size, Vector2 b_pos, Vector2 b_size)
        {
            if (a_pos.X < b_pos.X + b_size.X &&
                a_pos.X + a_size.X > b_pos.X &&
                a_pos.Y < b_pos.Y + b_size.Y &&
                a_pos.Y + a_size.Y > b_pos.Y)
            {
                return true;
            }
            return false;
        }*/
        public static bool IsColliding2D(Vector4 a, Vector4 b)
        {
            if (a.X < b.X + b.Z &&
                a.X + a.Z > b.X &&
                a.Y < b.Y + b.W &&
                a.Y + a.W > b.Y)
            {
                return true;
            }
            return false;
        }
        /*public static bool IsColliding2D(Vector4 a, Vector4 b)
        {
            if (a.X < b.X + b.Z &&
                a.X + a.Z > b.X &&
                a.Y < b.Y + b.Z &&
                a.Y + a.W > b.Y)
            {
                return true;
            }
            return false;
        }*/
        public static bool IsColliding2D(Sprite2D a, Sprite2D b)
        {
            if (a.Position.X < b.Position.X + b.Scale.X &&
                a.Position.X + a.Scale.X > b.Position.X &&
                a.Position.Y < b.Position.Y + b.Scale.Y &&
                a.Position.Y + a.Scale.Y > b.Position.Y)
            {
                return true;
            }

            return false;
        }

        public static bool IsColliding2D(Rectangle a, Rectangle b)
        {
            if (a.X < b.X + b.Width &&
                a.X + a.Width > b.X &&
                a.Y < b.Y + b.Height &&
                a.Y + a.Height > b.Y)
            {
                return true;
            }
            return false;
        }

        public static bool IsColliding2D(Rectangle a, Vector4 b)
        {
            if (a.X < b.X + b.Z &&
                a.X + a.Width > b.X &&
                a.Y < b.Y + b.W &&
                a.Y + a.Height > b.Y)
            {
                return true;
            }
            return false;
        }
    }
}
