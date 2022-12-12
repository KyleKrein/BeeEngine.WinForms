namespace BeeEngine.Drawing
{
    internal sealed class DrawnableChildrenCollection
    {
        List<DrawnableObject> children = new List<DrawnableObject>();

        private readonly object locker = new object();

        public void Add(DrawnableObject obj)
        {
            lock(locker)
            {
                if(!children.Contains(obj))
                    children.Add(obj);
            }
        }
        public void Remove(DrawnableObject obj)
        {
            lock(locker)
            {
                children.Remove(obj);
            }
        }
    }
}
