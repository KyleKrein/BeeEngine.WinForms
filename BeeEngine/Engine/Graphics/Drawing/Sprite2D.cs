using BeeEngine.GIF;
using BeeEngine.Vector;
using GameEngine2D;

namespace BeeEngine.Drawing
{
    public sealed class Sprite2D : DrawnableObject, IDisposable
    {
        private static byte _defaultPriority;

        private static bool _cacheLoaded;
        private static bool _cacheLoadingInProgress;
        private static readonly List<string> InProgress = new();

        private static readonly Dictionary<string, Bitmap[]> CachedGifs = new();

        private Bitmap _image;

        private byte _priority = _defaultPriority;

        private int _animIndex;
        private string _dir = "";
        private bool _disposedValue;
        private int _frameCount;
        private Bitmap[] _frames;

        //TODO:доделать луп
        public bool Loop = false;

        /*public void Destroy()
        {
            Hide();
        }*/

        public int MouseHoverTimer = 100;
        private double ms;
        internal bool started;

        //Animation stuff

        private int UpdateMS;
        private bool useArray;
        public WeakEvent<EventArgs> WeakAnimationEnds = new();

        public Sprite2D(Vector2 position, Vector2 scale, string directory, string tag, bool show = true)
        {
            Position = position;
            Scale = scale;
            Directory = directory;
            Tag = tag;
            Sprite = new Bitmap(_image!, (int) Scale.X, (int) Scale.Y);
            if (show)
            {
                Show();
            }
        }

        public Sprite2D(Vector2 position, Vector2 scale, Bitmap image, string tag, bool show = true)
        {
            Position = position;
            Scale = scale;
            Tag = tag;
            Sprite = new Bitmap(image, (int) Scale.X, (int) Scale.Y);
            if (show)
            {
                Show();
            }
        }

        public Sprite2D(Vector2 Position, Bitmap image, string Tag, bool show = true)
        {
            this.Position = Position;
            Scale = new Vector2(image.Width, image.Height);
            this.Tag = Tag;
            Sprite = new Bitmap(image);
            if (show)
            {
                Show();
            }
        }

        public Sprite2D(Vector2 Position, Vector2 Scale, string Directory, string Tag, byte Priority, bool Show = true)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Directory = Directory;
            this.Tag = Tag;

            Sprite = new Bitmap(_image, (int) this.Scale.X, (int) this.Scale.Y);
            this.Priority = Priority;
            if (Show)
            {
                this.Show();
            }
        }

        public Sprite2D(Vector2 Position, Vector2 Scale, Bitmap image, string Tag, byte Priority, bool Show = true)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Tag = Tag;
            Sprite = new Bitmap(image, (int) this.Scale.X, (int) this.Scale.Y);

            this.Priority = Priority;
            if (Show)
            {
                this.Show();
            }
        }

        public Sprite2D(Vector2 Position, Bitmap image, string Tag, byte Priority, bool Show = true)
        {
            this.Position = Position;
            Scale = new Vector2(image.Width, image.Height);
            this.Tag = Tag;
            Sprite = image;
            this.Priority = Priority;

            if (Show)
            {
                this.Show();
            }
        }

        public Vector2 Position { get; private set; }
        public Vector2 Scale { get; private set; }

        public Bitmap Sprite { get; private set; }
        //internal float camScale = Camera.Scale;

        public Transparency HasTransparency { get; set; } = Transparency.Semi;

        public string Directory
        {
            get => _dir;
            set
            {
                _dir = value;
                _image = GetImageFromDirectory(_dir);
            }
        }

        public string Name { get; set; }
        public string Tag { get; set; }

        public static byte DefaultPriority
        {
            get => _defaultPriority;
            set
            {
                if (value <= 4)
                {
                    _defaultPriority = value;
                }
                else
                {
                    throw new Exception("Default Priority must be between 0 and 4");
                }
            }
        }

        public byte Priority
        {
            get => _priority;
            set
            {
                if (IsDrawn)
                {
                    throw new Exception("Can't set priority when sprite is being showed");
                }

                if (value <= 4)
                {
                    _priority = value;
                }
                else
                {
                    throw new Exception("Sprite priority must be between 0 and 4");
                }
            }
        }

        public int FPS
        {
            get => 1000 / UpdateMS;
            set
            {
                if (value <= 1000)
                {
                    UpdateMS = 1000 / value;
                }
                else
                {
                    Log.Error("FPS is invalid");
                }
            }
        }

        public static string CacheFolder { get; set; } = Path.Combine(GameApplication.AssemblyDirectory, "Temp/");

        public static bool IsCachingInProgress => CountCachingGifs != 0;

        public static int CountCachingGifs { get; private set; }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override void Show()
        {
            if (GameApplication.Instance is null)
                throw new InvalidOperationException("Can't show sprite if game is not running");
            GameApplication.Instance.RenderingQueue.AddSprite(this);
            AddEvents();
            Log.Debug($"Sprite2D {Tag} is shown at {Position.X}:{Position.Y}");
            base.Show();
            //GameEngine.AddSprite(this);
        }

        public override void Hide()
        {
            if (GameApplication.Instance is null)
                throw new InvalidOperationException("Can't show sprite if game is not running");
            GameApplication.Instance.RenderingQueue.RemoveSprite(this);
            RemoveEvents();
            Log.Debug($"Sprite2D {Tag} is hidden at {Position.X}:{Position.Y}");
            base.Hide();
            //GameEngine.AddSprite(this);
        }
        public void Show(GameEngine gameInstance)
        {
            if (gameInstance is null)
                throw new ArgumentNullException(nameof(gameInstance));
            gameInstance.RenderingQueue.AddSprite(this);
            AddEvents(gameInstance);
            Log.Debug($"Sprite2D {Tag} is shown at {Position.X}:{Position.Y}");
            base.Show();
            //GameEngine.AddSprite(this);
        }

        public void Hide(GameEngine gameInstance)
        {
            if (gameInstance is null)
                throw new ArgumentNullException(nameof(gameInstance));
            gameInstance.RenderingQueue.RemoveSprite(this);
            RemoveEvents(gameInstance);
            Log.Debug($"Sprite2D {Tag} is hidden at {Position.X}:{Position.Y}");
            base.Hide();
            //GameEngine.AddSprite(this);
        }

        public void ChangePos(Vector2 Position, Vector2 Scale)
        {
            this.Position = Position;
            this.Scale = Scale;
            Sprite = new Bitmap(_image, (int) this.Scale.X, (int) this.Scale.Y);
            //Log.Info($"Позиция и размер спрайта {tag} были изменены");
        }

        public void ChangePos(Vector2 Position)
        {
            this.Position = Position;
        }

        public void ChangeImage(Bitmap image)
        {
            Sprite = image;
        }

        protected Bitmap GetImageFromDirectory(string dir)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Assets/" + dir + ".png");
            if (File.Exists(path))
            {
                return (Bitmap) Image.FromFile(path);
            }

            return null;
        }

        private void AddEvents()
        {
            AddEvents(GameApplication.Instance);
        }

        private void RemoveEvents()
        {
            RemoveEvents(GameApplication.Instance);
        }

        private void AddEvents(GameEngine gameInstance)
        {
            if (gameInstance is null)
                throw new ArgumentNullException(nameof(gameInstance));
            gameInstance.Window.MouseClick += Window_MouseClick;
            gameInstance.Window.MouseMove += Window_MouseMove;
            gameInstance.Window.MouseDown += Window_MouseDown;
        }
        private void RemoveEvents(GameEngine gameInstance)
        {
            if (gameInstance is null)
                throw new ArgumentNullException(nameof(gameInstance));
            gameInstance.Window.MouseClick -= Window_MouseClick;
            gameInstance.Window.MouseMove -= Window_MouseMove;
            gameInstance.Window.MouseDown -= Window_MouseDown;
        }

        private Vector2 CoordsForEvents(MouseEventArgs e)
        {
            return new Vector2(e.X - (Camera.Cameras[0].CenterPoint.X * Camera.Cameras[0].Scale) - Camera.Cameras[0].Position.X,
                e.Y - (Camera.Cameras[0].CenterPoint.Y * Camera.Cameras[0].Scale) - Camera.Cameras[0].Position.Y);
        }

        private void Window_MouseDown(object? sender, MouseEventArgs e)
        {
            Vector2 coords = CoordsForEvents(e);
            if (Collision.IsColliding2D(coords, Vector2.One(), Position, Scale))
            {
                MouseEventArgs eventArgs = new(e.Button, e.Clicks, (int) coords.X, (int) coords.Y, e.Delta);
                MouseDown?.Invoke(this, eventArgs);
                WeakMouseDown?.Invoke(this, eventArgs);
            }
        }

        public event EventHandler<FastGraphics> Paint;

        public event EventHandler<MouseEventArgs> MouseMove;

        public event EventHandler<MouseEventArgs> MouseClick;

        public event EventHandler<MouseEventArgs> MouseDown;

        public event EventHandler<MouseEventArgs> MouseUp;

        public event EventHandler<MouseEventArgs> MouseEnter;

        public event EventHandler<MouseEventArgs> MouseLeave;
        public event EventHandler<EventArgs> AnimationEnds;

        private void Window_MouseMove(object? sender, MouseEventArgs e)
        {
            Vector2 coords = CoordsForEvents(e);
            if (Collision.IsColliding2D(coords, Vector2.One(), Position, Scale))
            {
                MouseEventArgs eventArgs = new(e.Button, e.Clicks, (int) coords.X, (int) coords.Y, e.Delta);
                MouseMove?.Invoke(this, eventArgs);
                WeakMouseMove?.Invoke(this, eventArgs);
            }
        }

        private void Window_MouseClick(object? sender, MouseEventArgs e)
        {
            Vector2 coords = CoordsForEvents(e);
            if (Collision.IsColliding2D(coords, Vector2.One(), Position, Scale))
            {
                MouseEventArgs eventArgs = new(e.Button, e.Clicks, (int) coords.X, (int) coords.Y, e.Delta);
                MouseClick?.Invoke(this, eventArgs);
                WeakMouseClick?.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        ///     Starts animation using gif file (Can't work on Mac OS)
        /// </summary>
        /// <param name="gifImage"></param>
        /// <param name="name"></param>
        public async Task Animate(Bitmap gifImage, string name)
        {
            useArray = false;
#if !MAC
            //Logger.Info($"Can animate: {ImageAnimator.CanAnimate(gifImage)}");
            Sprite = gifImage;
            _frameCount = gifImage.GetFrameCount(new FrameDimension(gifImage.FrameDimensionsList[0]));
            ImageAnimator.Animate(Sprite, null);
            AnimateSprite();
#else
            useArray = true;
            string f = Path.Combine(cachePath, name);
            if (!CachedGifs.ContainsKey(f))
            {
                if(inProgress.Contains(f))
                {
                    await Task.Run(() =>
                    {
                        while (!CachedGifs.ContainsKey(f))
                        {
                            Thread.Sleep(500);
                        }
                    });
                }
                else
                {
                    await CacheGif(gifImage, f);
                }
            }
            frames = CachedGifs[f];
            frameCount = frames.Length;
            if (frames[0].Width != Scale.X || frames[0].Height != Scale.Y)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    frames[i] = new Bitmap(frames[i], (int)Scale.X, (int)Scale.Y);
                }
            }
            AnimateSprite();

            //throw new Exception("Can't work with GIF files on Mac OS. Use Bitmap array instead");
#endif

            ms = Time.time * 1000;
            started = true;
        }

        public void Animate(Bitmap[] framesArray)
        {
            useArray = true;
            _frames = framesArray;
            _frameCount = _frames.Length;
            if (_frames[0].Width != Scale.X || _frames[0].Height != Scale.Y)
            {
                for (int i = 0; i < _frameCount; i++)
                {
                    _frames[i] = new Bitmap(_frames[i], (int) Scale.X, (int) Scale.Y);
                }
            }

            AnimateSprite();
        }

        public void Stop()
        {
            if (!useArray)
            {
                ImageAnimator.StopAnimate(Sprite, null);
            }

            started = false;
            AnimationEnds?.Invoke(this, EventArgs.Empty);
            WeakAnimationEnds?.Invoke(this, EventArgs.Empty);
        }

        public void Resume()
        {
            started = true;
        }

        /*public void DeleteAnimation()
        {
            Stop();
            Sprite = original;
        }*/

        internal void Update()
        {
            if (started)
            {
                double t = Time.time * 1000;
                if (t - ms >= UpdateMS)
                {
                    AnimateSprite();

                    /*GameEngine.Window.Invoke((MethodInvoker)delegate
                    {
                        ImageAnimator.UpdateFrames(Sprite);
                    });*/

                    ms = t;
                }
            }
        }

        private void AnimateSprite()
        {
            if (useArray)
            {
                Sprite = _frames[_animIndex];
            }
            else
            {
                ImageAnimator.UpdateFrames(Sprite);
            }

            if (!Loop && _animIndex == _frameCount)
            {
                Stop();
            }

            _animIndex = _animIndex + 1 < _frameCount ? _animIndex + 1 : 0;
        }

        /*void getFrames(Image originalImg)
        {
            FrameDimension dimension = new FrameDimension(originalImg.FrameDimensionsList[0]);
            frameCount = originalImg.GetFrameCount(dimension);
            Image[] frames = new Image[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                originalImg.SelectActiveFrame(dimension, i);
                frames[i] = ((Image)originalImg.Clone());
            }

            Log.Info(frameCount.ToString());
            this.frames = frames;
        }*/
        /*private static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }*/

        //private static MemoryStream ReadGif(Stream inputStream, int frameIndex)
        //{
        //    var settings = new MagickReadSettings()
        //    {
        //        FrameIndex = 0,
        //        FrameCount = frameIndex + 1,
        //    };

        //    inputStream.Position = 0;
        //    using (var images = new MagickImageCollection(inputStream, settings))
        //    {
        //        images.Coalesce();

        //        var memoryStream = new MemoryStream();
        //        images[frameIndex].Write(memoryStream);
        //        return memoryStream;
        //    }
        //}
        /*Image[] getFrames(Image originalImg)
        {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            Image[] frames = new Image[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                frames[i] = ((Image)originalImg.Clone());
            }

            return frames;
        }*/

        private static Bitmap[] LoadGif(string name)
        {
            Bitmap[] frames;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Temp/");
            GifDecoder gifDecoder = new();
            System.IO.Directory.CreateDirectory(path);
            gifDecoder.Read(Path.Combine(path, name + ".gif"));
            int n = gifDecoder.GetFrameCount();
            frames = new Bitmap[n];
            for (int i = 0; i < n; i++)
            {
                frames[i] = (Bitmap) gifDecoder.GetFrame(i);
            }

            return frames;
        }

        private static void SaveGifToTemp(Bitmap gifImage, string name)
        {
            System.IO.Directory.CreateDirectory(CacheFolder);
            using (FileStream f = new(Path.Combine(CacheFolder, name), FileMode.Create))
            {
                gifImage.Save(f, ImageFormat.Gif);
            }
        }

        public static async Task CacheGif(Bitmap gifImage, string name)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            if (!_cacheLoaded)
            {
                if (!_cacheLoadingInProgress)
                {
                    LoadCachedGifs();
                }

                while (!_cacheLoaded)
                {
                    Thread.Sleep(20);
                }
            }

            stopwatch.Start();
            string f = Path.Combine(CacheFolder, name);
            if (!CachedGifs.ContainsKey(f) && !InProgress.Contains(f))
            {
                InProgress.Add(f);
                CountCachingGifs++;
                SaveGifToTemp(gifImage, name);
                Bitmap[] t = new Bitmap[1];
                await Task.Run(() =>
                { 
                    Log.Info($"Caching {name} in progress");
                    t = LoadGif(name);
                    SaveFramesToCache(t, name);
                });
                CachedGifs.Add(name, t);
                CountCachingGifs--;
            }

            TimeSpan ts = stopwatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Log.Debug($"Gif {name} was saved to cache: {elapsedTime}");
        }

        private static void LoadCachedGifs()
        {
            Log.Debug("Loading gifs from cache started");
            _cacheLoadingInProgress = true;
            System.IO.Directory.CreateDirectory(CacheFolder);
            string[] files = System.IO.Directory.GetFileSystemEntries(CacheFolder);
            IEnumerable<string> gifNames = files.Where(i => i.Contains(".gif"));
            CountCachingGifs += gifNames.Count();

            foreach (string item in gifNames)
            {
                string a = item.Split('.')[0];
                List<string> frames = files.Where(i => i.Contains(a) && !i.Contains(".gif")).OrderBy(i => i).ToList();
                //Logger.Info($"{frames.Count()}");
                Bitmap[] images = new Bitmap[frames.Count()];
                for (int i = 0; i < images.Length; i++)
                {
                    using (FileStream fs = new(frames[i], FileMode.Open))
                    {
                        images[i] = new Bitmap(fs);
                    }
                }

                CachedGifs.Add(a, images);
                Log.Debug($"{a} was loaded from cache");
                CountCachingGifs--;
            }

            _cacheLoadingInProgress = false;
            _cacheLoaded = true;
            Log.Debug("Loading gifs from cache completed");
        }

        private static void SaveFramesToCache(Bitmap[] f, string name)
        {
            int i = 0;
            foreach (Bitmap frame in f)
            {
                using (FileStream fs = new(Path.Combine(CacheFolder, name + i + ".png"), FileMode.Create))
                {
                    frame.Save(fs, ImageFormat.Png);
                }

                i++;
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (started)
                    {
                        Stop();
                    }

                    if (IsDrawn)
                    {
                        Hide();
                    }

                    InProgress.Clear();
                    _image?.Dispose();
                    Sprite.Dispose();
                    if (_frames != null)
                    {
                        foreach (Bitmap frame in _frames)
                        {
                            frame.Dispose();
                        }
                    }
                }

                Log.Debug($"Sprite2D {Tag} has been deleted at {Position.X}:{Position.Y}");
                _disposedValue = true;
            }
        }

        ~Sprite2D()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(false);
        }

        protected override void OnPaint(FastGraphics g)
        {
            //Update();
            g.DrawImage(Sprite, (int) Position.X, (int) Position.Y, HasTransparency);
            Paint?.Invoke(this, g);
            WeakPaint?.Invoke(this, g);
        }

        public static bool operator >(Sprite2D a, Sprite2D b)
        {
            return a.Priority > b.Priority;
        }

        public static bool operator <(Sprite2D a, Sprite2D b)
        {
            return a.Priority < b.Priority;
        }
    }
}