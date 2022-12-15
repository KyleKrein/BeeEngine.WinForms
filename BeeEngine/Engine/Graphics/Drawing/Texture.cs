namespace BeeEngine.Drawing;

public sealed class Texture : IDisposable
{
    private readonly Bitmap? _bitmap;
    private Dictionary<Size, int> _ids = new Dictionary<Size, int>();
    public int Width { get; private set; }
    public int Height { get; private set; }
    private int _isInvalid = 0;
    private Texture()
    {
        _bitmap = null;
    }

    public Texture(Image image)
    {
        _bitmap = new Bitmap(image);
        Width = image.Width;
        Height = image.Height;
    }

    public Texture(Image image, int width, int height)
    {
        _bitmap = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(_bitmap))
        {
            g.DrawImage(image, 0, 0);
            g.Flush();
        }
    }

    public static Texture FromFile(string filename)
    {
        return new Texture(Image.FromFile(filename));
    }

    internal Bitmap GetImageToDraw(int width, int height)
    {
        if (width == Width && height == Height)
            return _bitmap;
        Size size = new Size(width, height);
        if (_ids.TryGetValue(size, out var bitmapId))
        {
            return ImageCache.GetBitmap(bitmapId);
        }
        Bitmap final = new Bitmap(width, height);
        Graphics g = Graphics.FromImage(final);
        g.DrawImage(_bitmap, 0, 0, width, height);
        g.Dispose();
        int id = ImageCache.Add(final);
        _ids.Add(size, id);
        return final;
    }

    internal void Invalidate()
    {
        _isInvalid = -1;
    }

    internal IEnumerable<bool> HandleInvalidation()
    {
        if (_isInvalid != 0)
        {
            foreach (var id in _ids)
            {
                var bitmap = ImageCache.GetBitmap(id.Value);
                Graphics g = Graphics.FromImage(bitmap);
                g.Clear(Color.Transparent);
                g.DrawImage(_bitmap,0,0, id.Key.Width, id.Key.Height);
                g.Dispose();
                yield return true;
            }
            _isInvalid = 0;
        }
    }
    public void Dispose()
    {
        _bitmap?.Dispose();
        foreach (var id in _ids)
        {
            ImageCache.ReleaseImage(id.Value);
        }
    }
}