using BeeEngine.Vector;

namespace BeeEngine.Pathfinding
{
    public sealed class Node
    {
        public Node Parent;
        public Vector2 Position;
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        //насколько далеко от пункта назначения по цене и дистанции
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                return -1;
            }
        }
        public bool Walkable;

        public Node(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Weight = weight;
            Walkable = walkable;
        }

        public void Reset(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Weight = weight;
            Walkable = walkable;
        }
    }
}