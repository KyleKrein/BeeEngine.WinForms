using BeeEngine.Vector;
using GameEngine2D;

namespace BeeEngine.Drawing
{
    /// <summary>
    /// Optimized class that works with graphics, drawing and transformations
    /// </summary>
    public sealed class FastGraphics : IDisposable
    {
        private Graphics? _winFormGraphics;
        private Transform _transform;
        public ResamplingFilters ResamplingFilter
        {
            get
            {
                _resamplingService ??= new ResamplingService
                {
                    Filter = ResamplingFilters.Lanczos3
                };
                return _resamplingService.Filter;
            }
            set
            {
                _resamplingService ??= new ResamplingService();
                _resamplingService.Filter = value;
            }
        }
        public Graphics WinFormGraphics
        {
            get { return _winFormGraphics ??= Graphics.FromImage(_bitmap); }
        }
        private Bitmap? _bitmap;
        private bool disposedValue;
        private void Init()
        {
            _transform = Transform.Empty;
        }
        internal void Resize(int width, int height, ResamplingFilters filter)
        {
            _resamplingService.Filter = filter;
            _bitmap = _resamplingService.Resample(_bitmap.ToArray(), width, height).ConvertArrayToBitmap();
        }
        internal void Resize(int width, int height)
        {
            Resize(width, height, ResamplingFilters.CubicBSpline);
        }
        internal static void Resize(Bitmap bmp, int width, int height, ResamplingFilters filter)
        {
            var ResamplingService = new ResamplingService
            {
                Filter = filter
            };
            
            var newbmp = ResamplingService.Resample(bmp.ToArray(), width, height).ConvertArrayToBitmap();
            bmp.Dispose();
            bmp = newbmp;
        }
        internal static void Resize(Bitmap bmp, int width, int height)
        {
            Resize(bmp, width, height, ResamplingFilters.CubicBSpline);
        }
        internal static Bitmap ResizeAndGet(Bitmap bmp, int width, int height, ResamplingFilters filter)
        {
            var ResamplingService = new ResamplingService
            {
                Filter = filter
            };
            return ResamplingService.Resample(bmp.ToArray(), width, height).ConvertArrayToBitmap();
        }
        internal static Bitmap ResizeAndGet(Bitmap bmp, int width, int height)
        {
            return ResizeAndGet(bmp, width, height, ResamplingFilters.CubicBSpline);
        }
        public static FastGraphics FromImage(Bitmap image)
        {
            return new FastGraphics(image);
        }
        internal static FastGraphics FromImage(Bitmap image, Graphics graphics)
        {
            return new FastGraphics(graphics, image);
        }

        public void DrawImage(Bitmap image, int x, int y, Transparency transparency = Transparency.Semi)
        {
            Bitmap temp = image;
            int width = image.Width;
            int height = image.Height;

            DrawImage(temp, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), transparency);
        }

        public void DrawImage(Bitmap image, int x, int y, int width, int height, Transparency transparency = Transparency.Semi)
        {
            Bitmap temp = image;
            if(temp.Width!=width || temp.Height!=height)
            {
                temp = _resamplingService.Resample(image.ToArray(), width, height).ConvertArrayToBitmap();
            }
            DrawImage(temp, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), transparency);
        }

        public void DrawImage(Bitmap image, Rectangle position, Transparency transparency = Transparency.Semi)
        {
            DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), position, transparency);
        }

        internal bool IsFrameGraphics = false;
        public void DrawImage(Bitmap image, Rectangle imageRect, Rectangle newPositionRect, Transparency transparency = Transparency.Semi)
        {
            Rectangle changedRect = new Rectangle(newPositionRect.X, newPositionRect.Y, newPositionRect.Width, newPositionRect.Height);
            changedRect.X += (int)(_transform.X * _transform.ScaleX);
            changedRect.Y += (int)(_transform.Y * _transform.ScaleY);
            if (IsFrameGraphics)
            {
                if (HandleFrameGraphics(image, imageRect, newPositionRect, changedRect))
                {
                    return;
                }
            }
            
            switch (transparency)
            {
                case Transparency.None:
#if MAC
                    DrawSemiTransparentImage(image, changedRect.X, changedRect.Y);
#else

                    CopyImage(image, imageRect, changedRect);
#endif
                    break;
                case Transparency.Semi:
//#if MAC
//                    if (_bitmap != null)
//                    {
//                        FastBitmap.DrawRegion(image, _bitmap, imageRect, newPositionRect);
//                        return;
//                    }
//#endif
                    DrawSemiTransparentImage(image, changedRect.X, changedRect.Y);
                    break;
                case Transparency.Has:
                    if(_bitmap!=null)
                    {
                        FastBitmap.DrawRegion(image, _bitmap, imageRect, changedRect);
                        return;
                    }
                    DrawSemiTransparentImage(image, changedRect.X, changedRect.Y);
                    break;
            }
        }

        private bool HandleFrameGraphics(Bitmap image, Rectangle imageRect, Rectangle newPositionRect, Rectangle changedRect)
        {
            changedRect.Width = (int) (changedRect.Width * _transform.ScaleX);
            changedRect.Height = (int) (changedRect.Height * _transform.ScaleY);
            /*var display = new Rectangle((int) (-Camera.CenterPoint.X+_transform.X),
                (int) (-Camera.CenterPoint.Y+_transform.Y),
                (int) (GameApplication.Instance!.Window.ClientSize.Width),
                (int) (GameApplication.Instance!.Window.ClientSize.Height));*/
            /*if (!Collision.IsColliding2D(newPositionRect,Camera.Display) )
            {
                Log.Debug("Image was not processed");
                return true;
            }*/

            if (_transform.Scale.X == 1 && _transform.Scale.Y == 1)
            {
                return false;
            }

            /*imageRect.Width = (int) (imageRect.Width * _transform.ScaleX);
                    imageRect.Height = (int) (imageRect.Height * _transform.ScaleY);*/
            //temp = ResamplingService.Resample(image.ToArray(), imageRect.Width, imageRect.Height).ConvertArrayToBitmap();
            //ResamplingService.Resample(image, imageRect.Width, imageRect.Height);
            //temp = new Bitmap(imageRect.Width, imageRect.Height);
            Camera.Cameras.ForEach(camera => camera.Move(WinFormGraphics));/*.Move(WinFormGraphics);*/
            WinFormGraphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            WinFormGraphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            WinFormGraphics.DrawImage(image, newPositionRect.X, newPositionRect.Y/*, imageRect.Width, imageRect.Height*/);
            return true;
            /*
                    newPositionRect.Width = imageRect.Width;
                    newPositionRect.Height = imageRect.Height;*/

        }

        internal void CopyImage(Bitmap image, int x, int y)
        {
            int width = image.Width;
            int height = image.Height;
            CopyImage(image, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height));
        }

        internal void CopyImage(Bitmap image, int x, int y, int width, int height)
        {
            CopyImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(x, y, width, height));
        }
        ResamplingService? _resamplingService;
        internal void CopyImage(Bitmap image, Rectangle position)
        {
            CopyImage(image, new Rectangle(0, 0, image.Width, image.Height), position);
        }

        internal void CopyImage(Bitmap image, Rectangle imageRect, Rectangle newPositionRect)
        {
#if !MAC
            FastBitmap.CopyRegion(image, _bitmap, imageRect, newPositionRect);
#else
            WinFormGraphics.CompositingMode = CompositingMode.SourceCopy;
            WinFormGraphics.CompositingQuality = CompositingQuality.HighSpeed;
            WinFormGraphics.DrawImage(image, newPositionRect.X, newPositionRect.Y);
            WinFormGraphics.CompositingMode = CompositingMode.SourceOver;
            WinFormGraphics.CompositingQuality = CompositingQuality.Default;
#endif
        }

        public void DrawAndScaleImage(Bitmap image, int x, int y, int newWidth, int newHeight)
        {
            WinFormGraphics.DrawImage(image, x, y, newWidth, newHeight);
        }

        internal void DrawSemiTransparentImage(Bitmap image, int x, int y)
        {
            //WinFormGraphics.CompositingMode = CompositingMode.SourceOver;
            WinFormGraphics.DrawImage(image, x, y);
        }
        internal void DrawSemiTransparentImage(Bitmap image, int x, int y, int newWidth, int newHeight)
        {
            WinFormGraphics.DrawImage(image, x, y, newWidth, newHeight);
        }

        public void Clear(Color color)
        {
#if !MAC
            FastBitmap.ClearBitmap(_bitmap, color);
#else
            WinFormGraphics.Clear(color);
#endif
        }


        private FastGraphics()
        {
            Init();
        }
        private FastGraphics(Graphics winFormGraphics, Bitmap bitmap)
        {
            _winFormGraphics = winFormGraphics;
            _bitmap = bitmap;
            Init();
        }
        private FastGraphics(Bitmap bitmap)
        {
            _bitmap = bitmap;
            Init();
        }

        private FastGraphics(Graphics graphics)
        {
            _winFormGraphics= graphics;
            _bitmap = null;
            Init();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (WinFormGraphics != null)
                        WinFormGraphics.Dispose();
                    _bitmap = null;

                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~FastGraphics()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
        }

        internal static FastGraphics FromGraphics(Graphics graphics)
        {
            return new FastGraphics(graphics);
        }

        public void ResetTransform()
        {
            _transform.Reset();
        }
        public void TransformTranslate(int x, int y)
        {
            _transform.Translate(x, y);
        }
        public void TransformTranslate(Vector2 vector)
        {
            _transform.Translate(vector);
        }
        public void ScaleTransform(int x, int y)
        {
            _transform.ScaleTransform(x, y);
        }
        public void ScaleTransform(float x)
        {
            _transform.ScaleTransform(x);
        }
    }
}
