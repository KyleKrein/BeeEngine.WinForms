using BeeEngine.Vector;

namespace BeeEngine.Pathfinding
{
    public static class AStarPathfinder
    {

        public static List<Node> FindPath(Vector2 startpos, Vector2 endpos, Node[,] map)
        {
            Node start = new Node(startpos, true);
            Node end = new Node(endpos, true);

            List<Node> Path = new List<Node>();
            List<Node> OpenList = new List<Node>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Add(start);

            while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current, map);


                foreach (Node n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        if (!OpenList.Contains(n))
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Add(n);
                            OpenList = OpenList.OrderBy(node => node.F).ToList();
                        }
                    }
                }
            }
            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }
            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) { return null; }
            do
            {
                Path.Add(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);//do while это обычный цикл while. тут нужен do для проверки условия

            Path.Reverse();
            return Path;

        }
        private static List<Node> GetAdjacentNodes(Node n, Node[,] map)
        {
            List<Node> temp = new List<Node>();
            int MapRows = map.GetLength(1);
            int MapCols = map.GetLength(0);
            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if (row + 1 < MapRows)
            {
                temp.Add(map[col, row + 1]);
            }
            if (row - 1 >= 0)
            {
                temp.Add(map[col, row - 1]);
            }
            if (col - 1 >= 0)
            {
                temp.Add(map[col - 1, row]);
            }
            if (col + 1 < MapCols)
            {
                temp.Add(map[col + 1, row]);
            }

            return temp;
        }
    }
}
