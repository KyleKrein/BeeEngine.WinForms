using System.Collections.Concurrent;

namespace BeeEngine.Drawing;

public static class ImageCache
{
    private static int currentId = 0;
    //private static readonly ConcurrentDictionary<int, MemoryStream> images = new ConcurrentDictionary<int, MemoryStream>();
    private static readonly ConcurrentDictionary<int, Bitmap> images = new ConcurrentDictionary<int, Bitmap>();

    // stores removed ID to reuse
    private static ConcurrentQueue<int> recycle = new ConcurrentQueue<int>();
    private static readonly ReaderWriterLockSlim recycleLocker = new ReaderWriterLockSlim();
    
    
    /// <summary>
    /// Release an image based on its id.
    /// </summary>
    /// <param name="id"></param>
    public static void ReleaseImage(int id)
    {
        ReleaseMemoryStream(id);
        TryReset();
    }

    private static void ReleaseMemoryStream(int id)
    {
        //MemoryStream ms = null;
        Bitmap ms;
        if (images.TryRemove(id, out ms))
        {
            recycle.Enqueue(id);
            ms.Dispose();
        }
    }

    /// <summary>
    /// Releases all Images
    /// </summary>
    public static void ReleaseAllImages()
    {
        foreach (var id in images.Keys)
        {
            ReleaseMemoryStream(id);
        }

        TryReset();
    }

    /// <summary>
    /// Returns a Bitmap from the cache which is identified by an id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Bitmap GetBitmap(int id)
    {
        //MemoryStream ms = null;
        Bitmap ms;
        if (images.TryGetValue(id, out ms))
        {
            return ms; //(Bitmap) Image.FromStream(ms);
        }
        return null;
    }

    private static void TryReset()
    {
        if (!images.IsEmpty)
        {
            return;
        }

        //need to lock here
        if (recycleLocker.TryEnterWriteLock(TimeSpan.FromMilliseconds(100)))
        {
            try
            {
                // make sure another thread didn't sneak in and add another image
                if (images.IsEmpty)
                {
                    currentId = 0;
                    Interlocked.Exchange(ref recycle, new ConcurrentQueue<int>());
                }
            }
            finally
            {
                recycleLocker.ExitWriteLock();
            }
        }
    }
    
    /// <summary>
    /// Adds an Bitmap to the cache
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns>0 if the Bitmap is null, otherwise a unique id</returns>
    public static int Add(Bitmap bitmap)
    {
        if (bitmap == null)
        {
            return 0;
        }

        recycleLocker.EnterReadLock();

        try
        {
            //var ms = new MemoryStream();
            //bitmap.Save(ms, ImageFormat.Png);

            // Recycle Id or make new one
            int id;
            if (!recycle.TryDequeue(out id))
            {
                id = Interlocked.Increment(ref currentId);
            }

            // this should never be possible to fail
            images.TryAdd(id, /*ms*/bitmap);

            return id;
        }
        finally 
        { 
            recycleLocker.ExitReadLock();
        }
    }

    public static void Dispose()
    {
        recycleLocker.Dispose();
    }
}

/*public class ImageCache: IDisposable
{
    private int currentId = 0;
    private readonly ConcurrentDictionary<int, MemoryStream> images = new ConcurrentDictionary<int, MemoryStream>();
    // stores removed ID to reuse
    private ConcurrentQueue<int> recycle = new ConcurrentQueue<int>();
    private readonly ReaderWriterLockSlim recycleLocker = new ReaderWriterLockSlim();
    
    
    /// <summary>
    /// Release an image based on its id.
    /// </summary>
    /// <param name="id"></param>
    public void ReleaseImage(int id)
    {
        ReleaseMemoryStream(id);
        TryReset();
    }

    private void ReleaseMemoryStream(int id)
    {
        MemoryStream ms = null;
        if (images.TryRemove(id, out ms))
        {
            recycle.Enqueue(id);
            ms.Dispose();
        }
    }

    /// <summary>
    /// Releases all Images
    /// </summary>
    public void ReleaseAllImages()
    {
        foreach (var id in images.Keys)
        {
            ReleaseMemoryStream(id);
        }

        TryReset();
    }

    /// <summary>
    /// Returns a Bitmap from the cache which is identified by an id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Bitmap GetBitmap(int id)
    {
        MemoryStream ms = null;
        if (images.TryGetValue(id, out ms))
        {
            return (Bitmap) Image.FromStream(ms);
        }
        return null;
    }

    private void TryReset()
    {
        if (!images.IsEmpty)
        {
            return;
        }

        //need to lock here
        if (recycleLocker.TryEnterWriteLock(TimeSpan.FromMilliseconds(100)))
        {
            try
            {
                // make sure another thread didn't sneak in and add another image
                if (images.IsEmpty)
                {
                    currentId = 0;
                    Interlocked.Exchange(ref recycle, new ConcurrentQueue<int>());
                }
            }
            finally
            {
                recycleLocker.ExitWriteLock();
            }
        }
    }
    
    /// <summary>
    /// Adds an Bitmap to the cache
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns>0 if the Bitmap is null, otherwise a uique id</returns>
    public int Add(Bitmap bitmap)
    {
        if (bitmap == null)
        {
            return 0;
        }

        recycleLocker.EnterReadLock();

        try
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Tiff);

            // Recycle Id or make new one
            int id;
            if (!recycle.TryDequeue(out id))
            {
                id = Interlocked.Increment(ref currentId);
            }

            // this should never be possible to fail
            images.TryAdd(id, ms);

            return id;
        }
        finally 
        { 
            recycleLocker.ExitReadLock();
        }
    }

    public void Dispose()
    {
        recycleLocker.Dispose();
    }
}*/