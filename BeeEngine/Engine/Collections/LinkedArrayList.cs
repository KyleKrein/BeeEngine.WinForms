namespace BeeEngine.Collections
{
    public sealed class LinkedArrayList<T>
    {
        private class Node
        {
            internal ushort Length = 250;
            internal ushort Count;
            internal ushort Start = 0;
            internal T[] Array;

            public Node()
            {
                Array = new T[Length];
            }

            public Node(ushort len)
            {
                Length = len;
                Array = new T[Length];
            }

            public void Add(T item)
            {
                Array[Start + Count] = item;
                Count++;
            }
        }
    }
}