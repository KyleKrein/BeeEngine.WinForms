using System.Collections;

namespace BeeEngine.Collections
{
    public sealed class WeakCollection<T> : ICollection<T> where T : class
    {
        private readonly List<WeakReference<T>> list = new List<WeakReference<T>>();

        private readonly object locker = new object();

        public void Add(T item)
        {
            lock (locker)
            {
                list.Add(new WeakReference<T>(item));
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                list.Clear();
            }
        }

        public int Count
        {
            get
            {
                int i;
                lock (locker)
                {
                    i = list.Count;
                }
                return i;
            }
        }

        public bool IsReadOnly => false;

        public bool Contains(T item)
        {
            lock (locker)
            {
                foreach (var element in this)
                    if (Equals(element, item))
                        return true;
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                foreach (var element in this)
                    array[arrayIndex++] = element;
            }
        }

        public bool Remove(T item)
        {
            lock (locker)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].TryGetTarget(out T target))
                        continue;
                    if (Equals(target, item))
                    {
                        list.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
        }

        public bool RemoveAt(int index)
        {
            lock (locker)
            {
                list.RemoveAt(index);
                return true;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!list[i].TryGetTarget(out T element))
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    yield return element;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get
            {
                lock (locker)
                {
                    list[index].TryGetTarget(out T result);
                    return result;
                }
            }
            set
            {
                lock (locker)
                {
                    list[index].SetTarget(value);
                }
            }
        }

        internal void Sort(Comparison<T> comparison)
        {
            Comparer<T> comparer = Comparer<T>.Create(comparison);
            list.Sort((WeakReference<T> x, WeakReference<T> y) =>
            {
                x.TryGetTarget(out T a);
                x.TryGetTarget(out T b);
                return comparer.Compare(a, b);
            });
        }
    }
}