using System.Drawing.Text;

namespace BeeEngine.Drawing
{
    public static class ImageHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image ConvertToImage(this string text, string fontname, int fontsize, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                //задает качество отрисовки текста со сглаживанием
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                Font font = new Font(fontname, fontsize);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                //отрисовывает строку,используя только что созданные шрифт и кисть
                graphics.DrawString(text, font, new SolidBrush(fcolor), 0, 0);
                graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }
            return bmp;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ConvertToBitmap(this string text, string fontname, int fontsize, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                //задает качество отрисовки текста со сглаживанием
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                Font font = new Font(fontname, fontsize);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                //отрисовывает строку,используя только что созданные шрифт и кисть
                graphics.DrawString(text, font, new SolidBrush(fcolor), 0, 0);
                graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }
            return bmp;
        }
        public static Bitmap RotateAndGet(this Image b, float angle)

        {

            int maxside = (int)MathU.Sqrt(b.Width * b.Width + b.Height * b.Height);

            //create a new empty bitmap to hold rotated image

            Bitmap returnBitmap = new Bitmap(maxside, maxside);

            //make a graphics object from the empty bitmap

            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                //move rotation point to center of image

                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);

                //rotate

                g.RotateTransform(angle);

                //move image back

                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);

                //draw passed in image onto graphics object

                g.DrawImage(b, 0, 0);
                g.Flush();
                g.Dispose();
            }

            return returnBitmap;

        }
        public static void Rotate(this Image b, float angle)

        {

            int maxside = (int)MathU.Sqrt(b.Width * b.Width + b.Height * b.Height);

            //create a new empty bitmap to hold rotated image

            Bitmap returnBitmap = new Bitmap(maxside, maxside);

            //make a graphics object from the empty bitmap

            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                //move rotation point to center of image

                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);

                //rotate

                g.RotateTransform(angle);

                //move image back

                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);

                //draw passed in image onto graphics object

                g.DrawImage(b, 0, 0);
                g.Flush();
                g.Dispose();
            }
            b.Dispose();
            b = returnBitmap;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ColorMatrix CreateSepiaMatrix()
        {
            return new ColorMatrix(new[]
            { new[] { 0.393f, 0.349f, 0.272f, 0, 0 },
                                                    new[] { 0.769f, 0.686f, 0.534f, 0, 0 },
                                                    new[] { 0.189f, 0.168f, 0.131f, 0, 0 },
                                                    new float[] { 0, 0, 0, 1, 0 },
                                                    new float[] { 0, 0, 0, 0, 1 } });
        }
        /*private static ColorMatrix CreateSepiaMatrix()
        {
            return new ColorMatrix(new float[][] { new float[] { 0.393f, 0.349f, 0.272f, 0, 0 },
                                                    new float[] { 0.769f, 0.686f, 0.534f, 0, 0 },
                                                    new float[] { 0.189f, 0.168f, 0.131f, 0, 0 },
                                                    new float[] { 0, 0, 0, 1, 0 },
                                                    new float[] { -0.5f, -0.5f, -0.5f, 0, 1 } });
        }*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ToBlackAndWhite(this Image img, int brightness)
        {
            // Adjust brightness to be in range 0.0 - 1.0
            float bright = -1 * ((float)brightness / 255);

            // Average R, G, B values of all pixels
            float[][] colorTransMatrix =
            {
        new[] {0.333F, 0.333F, 0.333F, 0.000F, 0.000F},
        new[] {0.333F, 0.333F, 0.333F, 0.000F, 0.000F},
        new[] {0.333F, 0.333F, 0.333F, 0.000F, 0.000F},
        new[] {0.000F, 0.000F, 0.000F, 1.000F, 0.000F},
        new[] {bright, bright, bright, 0.000F, 1.000F},
    };
            Bitmap grayImg = translateBitmap(img, colorTransMatrix);

            // Return the grayscale image
            return grayImg;
        }
        private static Bitmap translateBitmap(Image img, float[][] colorTranslationMatrix)
        {
            // Setup color translation
            ColorMatrix colorMatrix = new ColorMatrix(colorTranslationMatrix);
            ImageAttributes imgAttr = new ImageAttributes();
            imgAttr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Draw the image with translated colors
            Bitmap trImg = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(trImg);
            g.DrawImage(img, new Rectangle(0, 0, trImg.Width, trImg.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttr);

            // Return the translated image
            return trImg;
        }
        public static Bitmap ToSepia(this Image img)
        {
            /*#if !MAC
                        Bitmap b = (Bitmap)bitmap;
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                var c = b.GetPixel(x, y);
                                //мат формула для покраски пикселя в "черно-белый" вариант
                                var rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                                b.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                            }
                        }
                        return bitmap;
            #else
                        /*Image img = bitmap;
                        ImageAttributes imageAttrs = new ImageAttributes();
                        imageAttrs.SetColorMatrix(CreateSepiaMatrix());
                        using (Graphics g = Graphics.FromImage(img))
                        {
                            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttrs);
                            g.Flush();
                            g.Dispose();
                        }
                        //return img;
                        return (Image)bitmap2Grayscale((Bitmap)bitmap, 0);
            #endif*/
            ImageAttributes imageAttrs = new ImageAttributes();
            imageAttrs.SetColorMatrix(CreateSepiaMatrix());
            using (Graphics g = Graphics.FromImage(img))
            {
                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttrs);
                g.Flush();
                g.Dispose();
            }
            return (Bitmap)img;
        }
        internal static void SetDoubleBuffered(Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (SystemInformation.TerminalServerSession)
                return;

            PropertyInfo aProp =
                typeof(Control).GetProperty(
                    "DoubleBuffered",
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }
    }
}
