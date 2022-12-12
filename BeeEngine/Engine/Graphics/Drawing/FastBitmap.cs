using BeeEngine.Engine.Drawing;
using BeeEngine.Vector;

namespace BeeEngine.Drawing
{
    /// <summary>
    /// Encapsulates a Bitmap for fast bitmap pixel operations using 32bpp images. Unsafe. Use FastGraphics instead.
    /// </summary>
    public sealed unsafe class FastBitmap : IDisposable
    {
        /// <summary>
        /// Specifies the number of bytes available per pixel of the bitmap object being manipulated
        /// </summary>
        public const int BytesPerPixel = 4;

        /// <summary>
        /// The Bitmap object encapsulated on this FastBitmap
        /// </summary>
        internal readonly Bitmap _bitmap;

        /// <summary>
        /// The BitmapData resulted from the lock operation
        /// </summary>
        private BitmapData _bitmapData;

        /// <summary>
        /// The first pixel of the bitmap
        /// </summary>
        private int* _scan0;

        /// <summary>
        /// Gets the width of this FastBitmap object
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of this FastBitmap object
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the pointer to the first pixel of the bitmap
        /// </summary>
        public IntPtr Scan0 => _bitmapData.Scan0;

        /// <summary>
        /// Gets the stride width (in int32-sized values) of the bitmap
        /// </summary>
        public int Stride { get; private set; }

        /// <summary>
        /// Gets the stride width (in bytes) of the bitmap
        /// </summary>
        public int StrideInBytes { get; private set; }

        /// <summary>
        /// Gets a boolean value that states whether this FastBitmap is currently locked in memory
        /// </summary>
        public bool Locked { get; private set; }

        /// <summary>
        /// Gets an array of 32-bit color pixel values that represent this FastBitmap.
        /// </summary>
        /// <exception cref="Exception">The locking operation required to extract the values off from the underlying bitmap failed</exception>
        /// <exception cref="InvalidOperationException">The bitmap is already locked outside this fast bitmap</exception>
        [Obsolete("DataArray property is deprecated. Please use GetDataAsArray() method instead.")]
        public int[] DataArray
        {
            get
            {
                bool unlockAfter = false;
                if (!Locked)
                {
                    Lock();
                    unlockAfter = true;
                }

                // Declare an array to hold the bytes of the bitmap
                int bytes = Math.Abs(_bitmapData.Stride) * _bitmap.Height;
                int[] argbValues = new int[bytes / BytesPerPixel];

                // Copy the RGB values into the array
                Marshal.Copy(_bitmapData.Scan0, argbValues, 0, bytes / BytesPerPixel);

                if (unlockAfter)
                {
                    Unlock();
                }

                return argbValues;
            }
        }

        /// <summary>
        /// Creates a new instance of the FastBitmap class with a specified Bitmap.
        /// The bitmap provided must have a 32bpp depth
        /// </summary>
        /// <param name="bitmap">The Bitmap object to encapsulate on this FastBitmap object</param>
        /// <exception cref="ArgumentException">The bitmap provided does not have a 32bpp pixel format</exception>
        public FastBitmap(Bitmap bitmap)
        {
            if (Image.GetPixelFormatSize(bitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap must have a 32bpp depth", nameof(bitmap));
            }

            _bitmap = bitmap;

            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        /// <summary>
        /// Disposes of this fast bitmap object and releases any pending resources.
        /// The underlying bitmap is not disposes, and is unlocked, if currently locked
        /// </summary>
        public void Dispose()
        {
            if (Locked)
            {
                Unlock();
            }
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations. If the bitmap is already locked,
        /// an exception is thrown
        /// </summary>
        /// <returns>A fast bitmap locked struct that will unlock the underlying bitmap after disposal</returns>
        /// <exception cref="InvalidOperationException">The bitmap is already locked</exception>
        /// <exception cref="System.Exception">The locking operation in the underlying bitmap failed</exception>
        /// <exception cref="InvalidOperationException">The bitmap is already locked outside this fast bitmap</exception>
        public FastBitmapLocker Lock()
        {
            return Lock((FastBitmapLockFormat)_bitmap.PixelFormat);
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations. If the bitmap is already locked,
        /// an exception is thrown.
        ///
        /// The provided pixel format should be a 32bpp format.
        /// </summary>
        /// <param name="pixelFormat">A pixel format to use when locking the underlying bitmap</param>
        /// <returns>A fast bitmap locked struct that will unlock the underlying bitmap after disposal</returns>
        /// <exception cref="InvalidOperationException">The bitmap is already locked</exception>
        /// <exception cref="Exception">The locking operation in the underlying bitmap failed</exception>
        /// <exception cref="InvalidOperationException">The bitmap is already locked outside this fast bitmap</exception>
        public FastBitmapLocker Lock(FastBitmapLockFormat pixelFormat)
        {
            if (Locked)
            {
                throw new InvalidOperationException("Unlock must be called before a Lock operation");
            }

            return Lock(ImageLockMode.ReadWrite, (PixelFormat)pixelFormat);
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations
        /// </summary>
        /// <param name="lockMode">The lock mode to use on the bitmap</param>
        /// <param name="pixelFormat">A pixel format to use when locking the underlying bitmap</param>
        /// <returns>A fast bitmap locked struct that will unlock the underlying bitmap after disposal</returns>
        /// <exception cref="System.Exception">The locking operation in the underlying bitmap failed</exception>
        /// <exception cref="InvalidOperationException">The bitmap is already locked outside this fast bitmap</exception>
        /// <exception cref="ArgumentException"><see cref="!:pixelFormat"/> is not a 32bpp format</exception>
        private FastBitmapLocker Lock(ImageLockMode lockMode, PixelFormat pixelFormat)
        {
            var rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);

            return Lock(lockMode, rect, pixelFormat);
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations
        /// </summary>
        /// <param name="lockMode">The lock mode to use on the bitmap</param>
        /// <param name="rect">The rectangle to lock</param>
        /// <param name="pixelFormat">A pixel format to use when locking the underlying bitmap</param>
        /// <returns>A fast bitmap locked struct that will unlock the underlying bitmap after disposal</returns>
        /// <exception cref="System.ArgumentException">The provided region is invalid</exception>
        /// <exception cref="System.Exception">The locking operation in the underlying bitmap failed</exception>
        /// <exception cref="InvalidOperationException">The bitmap region is already locked</exception>
        /// <exception cref="ArgumentException"><see cref="!:pixelFormat"/> is not a 32bpp format</exception>
        private FastBitmapLocker Lock(ImageLockMode lockMode, Rectangle rect, PixelFormat pixelFormat)
        {
            // Lock the bitmap's bits
            _bitmapData = _bitmap.LockBits(rect, lockMode, pixelFormat);

            _scan0 = (int*)_bitmapData.Scan0;
            Stride = _bitmapData.Stride / BytesPerPixel;
            StrideInBytes = _bitmapData.Stride;

            Locked = true;

            return new FastBitmapLocker(this);
        }

        /// <summary>
        /// Unlocks the bitmap and applies the changes made to it. If the bitmap was not locked
        /// beforehand, an exception is thrown
        /// </summary>
        /// <exception cref="InvalidOperationException">The bitmap is already unlocked</exception>
        /// <exception cref="System.Exception">The unlocking operation in the underlying bitmap failed</exception>
        public void Unlock()
        {
            if (!Locked)
            {
                throw new InvalidOperationException("Lock must be called before an Unlock operation");
            }

            _bitmap.UnlockBits(_bitmapData);

            Locked = false;
        }

        /// <summary>
        /// Sets the pixel color at the given coordinates. If the bitmap was not locked beforehand,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to set</param>
        /// <param name="y">The Y coordinate of the pixel to set</param>
        /// <param name="color">The new color of the pixel to set</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided coordinates are out of bounds of the bitmap</exception>
        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.ToArgb());
        }

        /// <summary>
        /// Sets the pixel color at the given coordinates. If the bitmap was not locked beforehand,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to set</param>
        /// <param name="y">The Y coordinate of the pixel to set</param>
        /// <param name="color">The new color of the pixel to set</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided coordinates are out of bounds of the bitmap</exception>
        public void SetPixel(int x, int y, int color)
        {
            SetPixel(x, y, unchecked((uint)color));
        }

        /// <summary>
        /// Sets the pixel color at the given coordinates. If the bitmap was not locked beforehand,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to set</param>
        /// <param name="y">The Y coordinate of the pixel to set</param>
        /// <param name="color">The new color of the pixel to set</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided coordinates are out of bounds of the bitmap</exception>
        public void SetPixel(int x, int y, uint color)
        {
            if (!Locked)
            {
                throw new InvalidOperationException("The FastBitmap must be locked before any pixel operations are made");
            }

            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), @"The X component must be >= 0 and < width");
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), @"The Y component must be >= 0 and < height");
            }

            *(uint*)(_scan0 + x + y * Stride) = color;
        }

        /// <summary>
        /// Sets the pixel color at the given index. If the bitmap was not locked beforehand,
        /// an exception is thrown
        /// </summary>
        /// <param name="index">An index into the underlying bitmap data, between <c>0 &lt;= index &lt; Height * Stride</c></param>
        /// <param name="color">The new color of the pixel to set</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided index is out of bounds of the bitmap</exception>
        public void SetPixel(int index, uint color)
        {
            if (!Locked)
            {
                throw new InvalidOperationException("The FastBitmap must be locked before any pixel operations are made");
            }

            if (index < 0 || index >= Height * Stride)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $@"The index must be >= 0 and < {nameof(Height)} * {nameof(Stride)}");
            }

            *(uint*)(_scan0 + index) = color;
        }

        /// <summary>
        /// Gets the pixel color at the given coordinates. If the bitmap was not locked beforehand,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to get</param>
        /// <param name="y">The Y coordinate of the pixel to get</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided coordinates are out of bounds of the bitmap</exception>
        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(GetPixelInt(x, y));
        }

        /// <summary>
        /// Gets the pixel color at the given coordinates as an integer value. If the bitmap
        /// was not locked beforehand, an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to get</param>
        /// <param name="y">The Y coordinate of the pixel to get</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided coordinates are out of bounds of the bitmap</exception>
        public int GetPixelInt(int x, int y)
        {
            if (!Locked)
            {
                throw new InvalidOperationException("The FastBitmap must be locked before any pixel operations are made");
            }

            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), @"The X component must be >= 0 and < width");
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), @"The Y component must be >= 0 and < height");
            }

            return *(_scan0 + x + y * Stride);
        }

        /// <summary>
        /// Gets the pixel color at the given coordinates as an unsigned integer value.
        /// If the bitmap was not locked beforehand, an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to get</param>
        /// <param name="y">The Y coordinate of the pixel to get</param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided coordinates are out of bounds of the bitmap</exception>
        public uint GetPixelUInt(int x, int y)
        {
            if (!Locked)
            {
                throw new InvalidOperationException("The FastBitmap must be locked before any pixel operations are made");
            }

            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), @"The X component must be >= 0 and < width");
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), @"The Y component must be >= 0 and < height");
            }

            return *((uint*)_scan0 + x + y * Stride);
        }

        /// <summary>
        /// Gets the pixel color at a given absolute index on the underlying bitmap data as
        /// an unsigned integer value.
        ///
        /// If the bitmap was not locked beforehand, an exception is thrown
        /// </summary>
        /// <param name="index">An index into the underlying bitmap data, between <c>0 &lt;= index &lt; Height * Stride</c></param>
        /// <exception cref="InvalidOperationException">The fast bitmap is not locked</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided index is out of bounds of the bitmap data</exception>
        public uint GetPixelUInt(int index)
        {
            if (!Locked)
            {
                throw new InvalidOperationException("The FastBitmap must be locked before any pixel operations are made");
            }

            if (index < 0 || index >= Height * Stride)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $@"The index must be >= 0 and < {nameof(Height)} * {nameof(Stride)}");
            }

            return *((uint*)_scan0 + index);
        }

        /// <summary>
        /// Copies the contents of the given array of colors into this FastBitmap.
        /// Throws an ArgumentException if the count of colors on the array mismatches the pixel count from this FastBitmap
        /// </summary>
        /// <param name="colors">The array of colors to copy</param>
        /// <param name="ignoreZeroes">Whether to ignore zeroes when copying the data</param>
        public void CopyFromArray(int[] colors, bool ignoreZeroes = false)
        {
            if (colors.Length != Width * Height)
            {
                throw new ArgumentException(@"The number of colors of the given array mismatch the pixel count of the bitmap", nameof(colors));
            }

            // Simply copy the argb values array
            // ReSharper disable once InconsistentNaming
            int* s0t = _scan0;

            fixed (int* source = colors)
            {
                // ReSharper disable once InconsistentNaming
                int* s0s = source;

                int count = Width * Height;

                if (!ignoreZeroes)
                {
                    // Unfold the loop
                    const int sizeBlock = 8;
                    int rem = count % sizeBlock;

                    count /= sizeBlock;

                    while (count-- > 0)
                    {
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);

                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                        *(s0t++) = *(s0s++);
                    }

                    while (rem-- > 0)
                    {
                        *(s0t++) = *(s0s++);
                    }
                }
                else
                {
                    while (count-- > 0)
                    {
                        if (*(s0s) == 0) { s0t++; s0s++; continue; }
                        *(s0t++) = *(s0s++);
                    }
                }
            }
        }

        /// <summary>
        /// Gets an array of 32-bit color pixel values that represent this FastBitmap.
        /// </summary>
        /// <exception cref="Exception">The locking operation required to extract the values off from the underlying bitmap failed</exception>
        /// <exception cref="InvalidOperationException">The bitmap is already locked outside this fast bitmap</exception>
        public int[] GetDataAsArray()
        {
            bool unlockAfter = false;
            if (!Locked)
            {
                Lock();
                unlockAfter = true;
            }

            // Declare an array to hold the bytes of the bitmap
            int bytes = Math.Abs(_bitmapData.Stride) * _bitmap.Height;
            int[] argbValues = new int[bytes / BytesPerPixel];

            // Copy the RGB values into the array
            Marshal.Copy(_bitmapData.Scan0, argbValues, 0, bytes / BytesPerPixel);

            if (unlockAfter)
            {
                Unlock();
            }

            return argbValues;
        }

        /// <summary>
        /// Clears the bitmap with the given color
        /// </summary>
        /// <param name="color">The color to clear the bitmap with</param>
        public void Clear(Color color)
        {
            Clear(color.ToArgb());
        }

        /// <summary>
        /// Clears the bitmap with the given color
        /// </summary>
        /// <param name="color">The color to clear the bitmap with</param>
        public void Clear(int color)
        {
            bool unlockAfter = false;
            if (!Locked)
            {
                Lock();
                unlockAfter = true;
            }

            // Clear all the pixels
            int count = Width * Height;
            int* curScan = _scan0;

            // Uniform color pixel values can be mem-set straight away
            int component = (color & 0xFF);
            if (component == ((color >> 8) & 0xFF) && component == ((color >> 16) & 0xFF) && component == ((color >> 24) & 0xFF))
            {
                memset(_scan0, component, (ulong)(Height * Stride * BytesPerPixel));
            }
            else
            {
                // Defines the ammount of assignments that the main while() loop is performing per loop.
                // The value specified here must match the number of assignment statements inside that loop
                const int assignsPerLoop = 8;

                int rem = count % assignsPerLoop;
                count /= assignsPerLoop;

                while (count-- > 0)
                {
                    *(curScan++) = color;
                    *(curScan++) = color;
                    *(curScan++) = color;
                    *(curScan++) = color;

                    *(curScan++) = color;
                    *(curScan++) = color;
                    *(curScan++) = color;
                    *(curScan++) = color;
                }
                while (rem-- > 0)
                {
                    *(curScan++) = color;
                }

                if (unlockAfter)
                {
                    Unlock();
                }
            }
        }

        /// <summary>
        /// Clears a square region of this image w/ a given color
        /// </summary>
        /// <param name="region"></param>
        /// <param name="color"></param>
        public void ClearRegion(Rectangle region, Color color)
        {
            ClearRegion(region, color.ToArgb());
        }

        /// <summary>
        /// Clears a square region of this image w/ a given color
        /// </summary>
        /// <param name="region"></param>
        /// <param name="color"></param>
        public void ClearRegion(Rectangle region, int color)
        {
            var thisReg = new Rectangle(0, 0, Width, Height);
            if (!region.IntersectsWith(thisReg))
                return;

            // If the region covers the entire image, use faster Clear().
            if (region == thisReg)
            {
                Clear(color);
                return;
            }

            int minX = region.X;
            int maxX = region.X + region.Width;

            int minY = region.Y;
            int maxY = region.Y + region.Height;

            // Bail out of optimization if there's too few rows to make this worth it
            if (maxY - minY < 16)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        *(_scan0 + x + y * Stride) = color;
                    }
                }
                return;
            }

            ulong strideWidth = (ulong)region.Width * BytesPerPixel;

            // Uniform color pixel values can be mem-set straight away
            int component = (color & 0xFF);
            if (component == ((color >> 8) & 0xFF) && component == ((color >> 16) & 0xFF) &&
                component == ((color >> 24) & 0xFF))
            {
                for (int y = minY; y < maxY; y++)
                {
                    memset(_scan0 + minX + y * Stride, component, strideWidth);
                }
            }
            else
            {
                // Prepare a horizontal slice of pixels that will be copied over each horizontal row down.
                var row = new int[region.Width];

                fixed (int* pRow = row)
                {
                    int count = region.Width;
                    int rem = count % 8;
                    count /= 8;
                    int* pSrc = pRow;
                    while (count-- > 0)
                    {
                        *pSrc++ = color;
                        *pSrc++ = color;
                        *pSrc++ = color;
                        *pSrc++ = color;

                        *pSrc++ = color;
                        *pSrc++ = color;
                        *pSrc++ = color;
                        *pSrc++ = color;
                    }
                    while (rem-- > 0)
                    {
                        *pSrc++ = color;
                    }

                    var sx = _scan0 + minX;
                    for (int y = minY; y < maxY; y++)
                    {
                        memcpy(sx + y * Stride, pRow, strideWidth);
                    }
                }
            }
        }

        /// <summary>
        /// Copies a region of the source bitmap into this fast bitmap
        /// </summary>
        /// <param name="source">The source image to copy</param>
        /// <param name="srcRect">The region on the source bitmap that will be copied over</param>
        /// <param name="destRect">The region on this fast bitmap that will be changed</param>
        /// <exception cref="ArgumentException">The provided source bitmap is the same bitmap locked in this FastBitmap</exception>
        public void CopyRegion(Bitmap source, Rectangle srcRect, Rectangle destRect)
        {
            // Throw exception when trying to copy same bitmap over
            if (source == _bitmap)
            {
                throw new ArgumentException(@"Copying regions across the same bitmap is not supported", nameof(source));
            }

            var srcBitmapRect = new Rectangle(0, 0, source.Width, source.Height);
            var destBitmapRect = new Rectangle(0, 0, Width, Height);

            // Check if the rectangle configuration doesn't generate invalid states or does not affect the target image
            if (srcRect.Width <= 0 || srcRect.Height <= 0 || destRect.Width <= 0 || destRect.Height <= 0 ||
                !srcBitmapRect.IntersectsWith(srcRect) || !destRect.IntersectsWith(destBitmapRect))
                return;

            // Find the areas of the first and second bitmaps that are going to be affected
            srcBitmapRect = Rectangle.Intersect(srcRect, srcBitmapRect);

            // Clip the source rectangle on top of the destination rectangle in a way that clips out the regions of the original bitmap
            // that will not be drawn on the destination bitmap for being out of bounds
            srcBitmapRect = Rectangle.Intersect(srcBitmapRect, new Rectangle(srcRect.X, srcRect.Y, destRect.Width, destRect.Height));

            destBitmapRect = Rectangle.Intersect(destRect, destBitmapRect);

            // Clip the source bitmap region yet again here
            srcBitmapRect = Rectangle.Intersect(srcBitmapRect, new Rectangle(-destRect.X + srcRect.X, -destRect.Y + srcRect.Y, Width, Height));

            // Calculate the rectangle containing the maximum possible area that is supposed to be affected by the copy region operation
            int copyWidth = Math.Min(srcBitmapRect.Width, destBitmapRect.Width);
            int copyHeight = Math.Min(srcBitmapRect.Height, destBitmapRect.Height);

            if (copyWidth == 0 || copyHeight == 0)
                return;

            int srcStartX = srcBitmapRect.Left;
            int srcStartY = srcBitmapRect.Top;

            int destStartX = destBitmapRect.Left;
            int destStartY = destBitmapRect.Top;

            //

            //BitmapData srcData = source.LockBits(srcBitmapRect, ImageLockMode.ReadOnly, source.PixelFormat);
            //BitmapData resData = _bitmap.LockBits(destBitmapRect, ImageLockMode.WriteOnly, _bitmap.PixelFormat);

            //int srcStride = srcData.Stride;
            //int resStride = resData.Stride;
            //int rowLength = copyWidth*BytesPerPixel;
            /*using (var fastSource = source.FastLock())
            {
                ulong strideWidth = (ulong)copyWidth * 4;
                
                Parallel.For(0, copyHeight, y =>
                {
                    byte[] buffer = new byte[strideWidth];
                    int destX = destStartX;
                    int destY = destStartY + y;

                    int srcX = srcStartX;
                    int srcY = srcStartY + y;

                    long offsetSrc = (srcX + srcY * fastSource.Stride);
                    long offsetDest = (destX + destY * Stride);
                    Marshal.Copy(new IntPtr(fastSource._scan0 + offsetSrc), buffer, 0, (int)strideWidth);
                    Marshal.Copy(buffer, 0, new IntPtr((this._scan0 + offsetDest)), (int)strideWidth);
                });/*
                    for (int y = 0; y < copyHeight; y++)
                {
                   
                }*/
            //}

            using (var fastSource = source.FastLock())
            {
                ulong strideWidth = (ulong)copyWidth * BytesPerPixel;

                // Perform copies of whole pixel rows
                for (int y = 0; y < copyHeight; y++)
                {
                    int destX = destStartX;
                    int destY = destStartY + y;

                    int srcX = srcStartX;
                    int srcY = srcStartY + y;

                    long offsetSrc = (srcX + srcY * fastSource.Stride);
                    long offsetDest = (destX + destY * Stride);

                    memcpy(_scan0 + offsetDest, fastSource._scan0 + offsetSrc, strideWidth);
                }
            }
        }

        public static Vector4 RGBToLab(Vector4 color)
        {
            float[] xyz = new float[3];
            float[] lab = new float[3];
            float[] rgb = { color[0], color[1], color[2], color[3] };

            rgb[0] = color[0] / 255.0f;
            rgb[1] = color[1] / 255.0f;
            rgb[2] = color[2] / 255.0f;

            if (rgb[0] > .04045f)
            {
                rgb[0] = (float)Math.Pow((rgb[0] + .0055) / 1.055, 2.4);
            }
            else
            {
                rgb[0] = rgb[0] / 12.92f;
            }

            if (rgb[1] > .04045f)
            {
                rgb[1] = (float)Math.Pow((rgb[1] + .0055) / 1.055, 2.4);
            }
            else
            {
                rgb[1] = rgb[1] / 12.92f;
            }

            if (rgb[2] > .04045f)
            {
                rgb[2] = (float)Math.Pow((rgb[2] + .0055) / 1.055, 2.4);
            }
            else
            {
                rgb[2] = rgb[2] / 12.92f;
            }
            rgb[0] = rgb[0] * 100.0f;
            rgb[1] = rgb[1] * 100.0f;
            rgb[2] = rgb[2] * 100.0f;


            xyz[0] = ((rgb[0] * .412453f) + (rgb[1] * .357580f) + (rgb[2] * .180423f));
            xyz[1] = ((rgb[0] * .212671f) + (rgb[1] * .715160f) + (rgb[2] * .072169f));
            xyz[2] = ((rgb[0] * .019334f) + (rgb[1] * .119193f) + (rgb[2] * .950227f));


            xyz[0] = xyz[0] / 95.047f;
            xyz[1] = xyz[1] / 100.0f;
            xyz[2] = xyz[2] / 108.883f;

            if (xyz[0] > .008856f)
            {
                xyz[0] = (float)Math.Pow(xyz[0], (1.0 / 3.0));
            }
            else
            {
                xyz[0] = (xyz[0] * 7.787f) + (16.0f / 116.0f);
            }

            if (xyz[1] > .008856f)
            {
                xyz[1] = (float)Math.Pow(xyz[1], 1.0 / 3.0);
            }
            else
            {
                xyz[1] = (xyz[1] * 7.787f) + (16.0f / 116.0f);
            }

            if (xyz[2] > .008856f)
            {
                xyz[2] = (float)Math.Pow(xyz[2], 1.0 / 3.0);
            }
            else
            {
                xyz[2] = (xyz[2] * 7.787f) + (16.0f / 116.0f);
            }

            lab[0] = (116.0f * xyz[1]) - 16.0f;
            lab[1] = 500.0f * (xyz[0] - xyz[1]);
            lab[2] = 200.0f * (xyz[1] - xyz[2]);

            return new Vector4(lab[0], lab[1], lab[2], color[3]);
        }
        public static Vector4 LabtoXYZ(Vector4 LAB)
        {
            double delta = 6.0 / 29.0;

            double fy = (LAB.X + 16) / 116.0;
            double fx = fy + (LAB.Y / 500.0);
            double fz = fy - (LAB.Z / 200.0);

            return new Vector4(
                (float)((fx > delta) ? CIEXYZ.D65.X * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.X),
                (float)((fy > delta) ? CIEXYZ.D65.Y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.Y),
                (float)((fz > delta) ? CIEXYZ.D65.Z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (
                    delta * delta) * CIEXYZ.D65.Z)
                , LAB.W);
        }
        public static Vector4 XYZtoRGB(Vector4 XYZ)
        {
            double[] Clinear = new double[3];
            Clinear[0] = XYZ.X * 3.2410 - XYZ.Y * 1.5374 - XYZ.Z * 0.4986; // red
            Clinear[1] = -XYZ.X * 0.9692 + XYZ.Y * 1.8760 - XYZ.Z * 0.0416; // green
            Clinear[2] = XYZ.X * 0.0556 - XYZ.Y * 0.2040 + XYZ.Z * 1.0570; // blue

            for (int i = 0; i < 3; i++)
            {
                Clinear[i] = (Clinear[i] <= 0.0031308) ? 12.92 * Clinear[i] : (
                    1 + 0.055) * Math.Pow(Clinear[i], (1.0 / 2.4)) - 0.055;
            }

            return new Vector4(
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[0] * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[1] * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[2] * 255.0))),
                XYZ.W
                );
        }
        public static Vector4 LabtoRGB(Vector4 LAB)
        {
            return XYZtoRGB(LabtoXYZ(LAB));
        }


        public static void DrawRegion(Bitmap source, Bitmap target, Rectangle srcRect, Rectangle destRect)
        {

            // Throw exception when trying to copy same bitmap over
            if (source == target)
            {
                throw new ArgumentException(@"Copying regions across the same bitmap is not supported", nameof(source));
            }

            var srcBitmapRect = new Rectangle(0, 0, source.Width, source.Height);
            var destBitmapRect = new Rectangle(0, 0, target.Width, target.Height);

            // Check if the rectangle configuration doesn't generate invalid states or does not affect the target image
            if (srcRect.Width <= 0 || srcRect.Height <= 0 || destRect.Width <= 0 || destRect.Height <= 0 ||
                !srcBitmapRect.IntersectsWith(srcRect) || !destRect.IntersectsWith(destBitmapRect))
                return;

            // Find the areas of the first and second bitmaps that are going to be affected
            srcBitmapRect = Rectangle.Intersect(srcRect, srcBitmapRect);

            // Clip the source rectangle on top of the destination rectangle in a way that clips out the regions of the original bitmap
            // that will not be drawn on the destination bitmap for being out of bounds
            srcBitmapRect = Rectangle.Intersect(srcBitmapRect, new Rectangle(srcRect.X, srcRect.Y, destRect.Width, destRect.Height));

            destBitmapRect = Rectangle.Intersect(destRect, destBitmapRect);

            // Clip the source bitmap region yet again here
            srcBitmapRect = Rectangle.Intersect(srcBitmapRect, new Rectangle(-destRect.X + srcRect.X, -destRect.Y + srcRect.Y, target.Width, target.Height));

            // Calculate the rectangle containing the maximum possible area that is supposed to be affected by the copy region operation
            int copyWidth = srcBitmapRect.Width.Min(destBitmapRect.Width);
            int copyHeight = srcBitmapRect.Height.Min(destBitmapRect.Height);

            if (copyWidth == 0 || copyHeight == 0)
                return;

            int srcStartX = srcBitmapRect.Left;
            int srcStartY = srcBitmapRect.Top;

            int destStartX = destBitmapRect.Left;
            int destStartY = destBitmapRect.Top;
            using (FastBitmap fastTarget = target.FastLock(), fastSource = source.FastLock())
            {

                byte* dstPointer = (byte*)fastTarget._scan0;
                byte* srcPointer = (byte*)fastSource._scan0;

                for (int y = 0; y < copyHeight; y++)
                {
                    int destX = destStartX;
                    int destY = destStartY + y;

                    int srcX = srcStartX;
                    int srcY = srcStartY + y;

                    int offsetSrc = (srcX + srcY * fastSource.Stride);
                    int offsetDest = (destX + destY * fastTarget.Stride);
                    srcPointer = (byte*)(fastSource._scan0 + offsetSrc);
                    dstPointer = (byte*)(fastTarget._scan0 + offsetDest);
                    for (int x = 0; x < copyWidth; x++)
                    {
                        if (srcPointer[3] != 0)
                        {
                            if (srcPointer[3] != byte.MaxValue)
                            {
                                //var LabSRC = RGBToLab(new Vector4(srcPointer[0], srcPointer[1], srcPointer[2], srcPointer[3]));
                                //var LabDST = RGBToLab(new Vector4(dstPointer[0], dstPointer[1], dstPointer[2], dstPointer[3]));
                                //var FinalRGB = LabtoRGB(new Vector4((LabSRC.X + LabDST.X)/2, (LabSRC.Y + LabDST.Y) / 2, (LabSRC.Z + LabDST.Z) / 2, (LabSRC.W + LabDST.W) / 2));
                                byte blue = (byte)((srcPointer[0]  + dstPointer[0]) / 2);
                                byte green = (byte)((srcPointer[1] + dstPointer[1]) / 2);
                                byte red = (byte)((srcPointer[2] + dstPointer[2]) / 2);
                                byte alpha = (byte)((srcPointer[3] + dstPointer[3]) / 2);
                                dstPointer[0] = blue;  //(byte)FinalRGB.X;
                                dstPointer[1] = green; //(byte)FinalRGB.Y;
                                dstPointer[2] = red; //(byte)FinalRGB.Z;
                                dstPointer[3] = alpha; //(byte)FinalRGB.W;
                            }
                            else
                            {
                                dstPointer[0] = srcPointer[0]; // Blue
                                dstPointer[1] = srcPointer[1]; // Green
                                dstPointer[2] = srcPointer[2]; // Red
                                dstPointer[3] = srcPointer[3]; // Alpha
                            }
                        }
                        

                        srcPointer += BytesPerPixel;
                        dstPointer += BytesPerPixel;
                    }

                }
            }
        }

        /// <summary>
        /// Performs a copy operation of the pixels from the Source bitmap to the Target bitmap.
        /// If the dimensions or pixel depths of both images don't match, the copy is not performed
        /// </summary>
        /// <param name="source">The bitmap to copy the pixels from</param>
        /// <param name="target">The bitmap to copy the pixels to</param>
        /// <returns>Whether the copy proceedure was successful</returns>
        /// <exception cref="ArgumentException">The provided source and target bitmaps are the same</exception>
        public static bool CopyPixels(Bitmap source, Bitmap target)
        {
            if (source == target)
            {
                throw new ArgumentException(@"Copying pixels across the same bitmap is not supported", nameof(source));
            }

            if (source.Width != target.Width || source.Height != target.Height || source.PixelFormat != target.PixelFormat)
                return false;

            using (FastBitmap fastSource = source.FastLock(),
                fastTarget = target.FastLock())
            {
                memcpy(fastTarget.Scan0, fastSource.Scan0, (ulong)(fastSource.Height * fastSource.Stride * BytesPerPixel));
            }


            /*BitmapData bitmapDataSRC = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData bitmapDataDST = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.WriteOnly, target.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(source.PixelFormat) / 8;
            int heightInPixels = bitmapDataSRC.Height;
            int widthInBytes = bitmapDataSRC.Width * bytesPerPixel;
            byte* PtrFirstPixelSRC = (byte*)bitmapDataSRC.Scan0;
            byte* PtrFirstPixelDST = (byte*)bitmapDataDST.Scan0;

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLineSRC = PtrFirstPixelSRC + (y * bitmapDataSRC.Stride);
                byte* currentLineDST = PtrFirstPixelDST + (y * bitmapDataDST.Stride);

                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = currentLineSRC[x];
                    int oldGreen = currentLineSRC[x + 1];
                    int oldRed = currentLineSRC[x + 2];

                    currentLineDST[x] = (byte)oldBlue;
                    currentLineDST[x + 1] = (byte)oldGreen;
                    currentLineDST[x + 2] = (byte)oldRed;
                }
            });
            source.UnlockBits(bitmapDataSRC);
            target.UnlockBits(bitmapDataDST);*/

            /*
            Rectangle bmpBounds = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData srcData = source.LockBits(bmpBounds, ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData resData = target.LockBits(bmpBounds, ImageLockMode.WriteOnly, target.PixelFormat);

            Int64 srcScan0 = srcData.Scan0.ToInt64();
            Int64 resScan0 = resData.Scan0.ToInt64();
            int srcStride = srcData.Stride;
            int resStride = resData.Stride;
            int rowLength = Math.Abs(srcData.Stride);
            try
            {
                byte[] buffer = new byte[rowLength];
                Parallel.For(0, srcData.Height, y =>
                {
                    Marshal.Copy(new IntPtr(srcScan0 + y * srcStride), buffer, 0, rowLength);
                    Marshal.Copy(buffer, 0, new IntPtr(resScan0 + y * resStride), rowLength);
                });
                    /*for (int y = 0; y < srcData.Height; y++)
                {
                    
                }*/
            /*}
            finally
            {
                source.UnlockBits(srcData);
                target.UnlockBits(resData);
            }*/

            return true;
        }

        /// <summary>
        /// Clears the given bitmap with the given color
        /// </summary>
        /// <param name="bitmap">The bitmap to clear</param>
        /// <param name="color">The color to clear the bitmap with</param>
        public static void ClearBitmap(Bitmap bitmap, Color color)
        {
            ClearBitmap(bitmap, color.ToArgb());
        }

        /// <summary>
        /// Clears the given bitmap with the given color
        /// </summary>
        /// <param name="bitmap">The bitmap to clear</param>
        /// <param name="color">The color to clear the bitmap with</param>
        public static void ClearBitmap(Bitmap bitmap, int color)
        {
            using (var fb = bitmap.FastLock())
            {
                fb.Clear(color);
            }
        }

        /// <summary>
        /// Copies a region of the source bitmap to a target bitmap
        /// </summary>
        /// <param name="source">The source image to copy</param>
        /// <param name="target">The target image to be altered</param>
        /// <param name="srcRect">The region on the source bitmap that will be copied over</param>
        /// <param name="destRect">The region on the target bitmap that will be changed</param>
        /// <exception cref="ArgumentException">The provided source and target bitmaps are the same bitmap</exception>
        public static void CopyRegion(Bitmap source, Bitmap target, Rectangle srcRect, Rectangle destRect)
        {
            var srcBitmapRect = new Rectangle(0, 0, source.Width, source.Height);
            var destBitmapRect = new Rectangle(0, 0, target.Width, target.Height);

            // If the copy operation results in an entire copy, use CopyPixels instead
            if (srcBitmapRect == srcRect && destBitmapRect == destRect && srcBitmapRect == destBitmapRect)
            {
                CopyPixels(source, target);
                return;
            }

            using (var fastTarget = target.FastLock())
            {
                fastTarget.CopyRegion(source, srcRect, destRect);
            }
        }

        /// <summary>
        /// Returns a bitmap that is a slice of the original provided 32bpp Bitmap.
        /// The region must have a width and a height > 0, and must lie inside the source bitmap's area
        /// </summary>
        /// <param name="source">The source bitmap to slice</param>
        /// <param name="region">The region of the source bitmap to slice</param>
        /// <returns>A Bitmap that represents the rectangle region slice of the source bitmap</returns>
        /// <exception cref="ArgumentException">The provided bimap is not 32bpp</exception>
        /// <exception cref="ArgumentException">The provided region is invalid</exception>
        public static Bitmap SliceBitmap(Bitmap source, Rectangle region)
        {
            if (region.Width <= 0 || region.Height <= 0)
            {
                throw new ArgumentException(@"The provided region must have a width and a height > 0", nameof(region));
            }

            var sliceRectangle = Rectangle.Intersect(new Rectangle(Point.Empty, source.Size), region);

            if (sliceRectangle.IsEmpty)
            {
                throw new ArgumentException(@"The provided region must not lie outside of the bitmap's region completely", nameof(region));
            }

            var slicedBitmap = new Bitmap(sliceRectangle.Width, sliceRectangle.Height);
            CopyRegion(source, slicedBitmap, sliceRectangle, new Rectangle(0, 0, sliceRectangle.Width, sliceRectangle.Height));

            return slicedBitmap;
        }

#if NETSTANDARD
        public static void memcpy(IntPtr dest, IntPtr src, ulong count)
        {
            Buffer.MemoryCopy(src.ToPointer(), dest.ToPointer(), count, count);
        }

        public static void memcpy(void* dest, void* src, ulong count)
        {
            Buffer.MemoryCopy(src, dest, count, count);
        }

        public static void memset(void* dest, int value, ulong count)
        {
            Unsafe.InitBlock(dest, (byte)value, (uint)count);
        }
#else
        /// <summary>
        /// .NET wrapper to native call of 'memcpy'. Requires Microsoft Visual C++ Runtime installed
        /// </summary>
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, ulong count);

        /// <summary>
        /// .NET wrapper to native call of 'memcpy'. Requires Microsoft Visual C++ Runtime installed
        /// </summary>
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(void* dest, void* src, ulong count);

        /// <summary>
        /// .NET wrapper to native call of 'memset'. Requires Microsoft Visual C++ Runtime installed
        /// </summary>
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memset(void* dest, int value, ulong count);
#endif

        /// <summary>
        /// Represents a disposable structure that is returned during Lock() calls, and unlocks the bitmap on Dispose calls
        /// </summary>
        public readonly struct FastBitmapLocker : IDisposable
        {
            /// <summary>
            /// Gets the fast bitmap instance attached to this locker
            /// </summary>
            public FastBitmap FastBitmap { get; }

            /// <summary>
            /// Initializes a new instance of the FastBitmapLocker struct with an initial fast bitmap object.
            /// The fast bitmap object passed will be unlocked after calling Dispose() on this struct
            /// </summary>
            /// <param name="fastBitmap">A fast bitmap to attach to this locker which will be released after a call to Dispose</param>
            public FastBitmapLocker(FastBitmap fastBitmap)
            {
                FastBitmap = fastBitmap;
            }

            /// <summary>
            /// Disposes of this FastBitmapLocker, essentially unlocking the underlying fast bitmap
            /// </summary>
            public void Dispose()
            {
                if (FastBitmap.Locked)
                    FastBitmap.Unlock();
            }
        }
    }

    /// <summary>
    /// Describes a pixel format to use when locking a bitmap using <see cref="FastBitmap"/>.
    /// </summary>
    public enum FastBitmapLockFormat
    {
        /// <summary>Specifies that the format is 32 bits per pixel; 8 bits each are used for the red, green, and blue components. The remaining 8 bits are not used.</summary>
        Format32bppRgb = 139273,
        /// <summary>Specifies that the format is 32 bits per pixel; 8 bits each are used for the alpha, red, green, and blue components. The red, green, and blue components are premultiplied, according to the alpha component.</summary>
        Format32bppPArgb = 925707,
        /// <summary>Specifies that the format is 32 bits per pixel; 8 bits each are used for the alpha, red, green, and blue components.</summary>
        Format32bppArgb = 2498570,
    }

    /// <summary>
    /// Static class that contains fast bitmap extension methdos for the Bitmap class
    /// </summary>
    public static class FastBitmapExtensions
    {
        /// <summary>
        /// Locks this bitmap into memory and returns a FastBitmap that can be used to manipulate its pixels
        /// </summary>
        /// <param name="bitmap">The bitmap to lock</param>
        /// <returns>A locked FastBitmap</returns>
        public static FastBitmap FastLock(this Bitmap bitmap)
        {
            var fast = new FastBitmap(bitmap);
            fast.Lock();

            return fast;
        }

        /// <summary>
        /// Locks this bitmap into memory and returns a FastBitmap that can be used to manipulate its pixels
        /// </summary>
        /// <param name="bitmap">The bitmap to lock</param>
        /// <param name="lockFormat">The underlying pixel format to use when locking the bitmap</param>
        /// <returns>A locked FastBitmap</returns>
        public static FastBitmap FastLock(this Bitmap bitmap, FastBitmapLockFormat lockFormat)
        {
            var fast = new FastBitmap(bitmap);
            fast.Lock(lockFormat);

            return fast;
        }

        /// <summary>
        /// Returns a deep clone of this Bitmap object, with all the data copied over.
        /// After a deep clone, the new bitmap is completely independent from the original
        /// </summary>
        /// <param name="bitmap">The bitmap to clone</param>
        /// <returns>A deep clone of this Bitmap object, with all the data copied over</returns>
        public static Bitmap DeepClone(this Bitmap bitmap)
        {
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        }
    }
}
