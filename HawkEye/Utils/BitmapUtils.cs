using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HawkEye.Utils
{
    internal class BitmapUtils
    {
        public static void Scale(ref Bitmap bitmap, int scale) => bitmap = new Bitmap(bitmap, new Size(bitmap.Width * scale, bitmap.Height * scale));

        public static void Resize(ref Bitmap bitmap, int width, int height) => bitmap = new Bitmap(bitmap, new Size(width, height));

        //TODO: Add setting to choose between different grayscaling techniques
        //Thanks to Aeolin for this fast piece of pointer magic!
        //Even more performant than GrayscaleWithLockBits
        /// <summary>
        /// Converts an image to grayscale extremely fast
        /// </summary>
        /// <param name="bitmap">The bitmap to be grayscaled</param>
        /// <param name="balance">float array containing balance for grayscale influenced ordered r,g,b</param>
        /// <param name="stepSize">How many Pixels should be processed in one go, has to be divisible by bytes/pixel</param>
        public static void GrayScaleByAwoTechnique(Bitmap bitmap, float[] balance = null, int stepSize = 128)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0.ToPointer();
                int bytesPerPixel = data.Stride / data.Width;
                var len = (bitmap.Size.Width * bitmap.Size.Height);
                if (stepSize % bytesPerPixel != 0)
                {
                    bitmap.UnlockBits(data); // clean up
                    throw new ArgumentException($"stepSize {stepSize} is not divisible by bytes per pixel {bytesPerPixel} for this bitmap", nameof(stepSize));
                }

                Enumerable.Range(0, len / stepSize)
                  .Select(x => (off: x * stepSize * bytesPerPixel, len: Math.Min(len - ((x + 1) * stepSize), stepSize)))
                  .AsParallel()
                  .ForAll(x => GrayScaleRange(ptr + x.off, x.len, balance ?? new float[] { 0.30f, 0.59f, 0.11f }, bytesPerPixel));
            }

            bitmap.UnlockBits(data);
        }

        private static unsafe void GrayScaleRange(byte* data, int length, float[] balance, int bytesPerPixel)
        {
            while (length-- > 0)
            {
                var gray = (byte)(((*(data + 2)) * balance[0]) + ((*(data + 1)) * balance[1]) + ((*(data + 0)) * balance[2]));

                *data++ = gray;
                *data++ = gray;
                *data++ = gray;

                if (bytesPerPixel == 4)
                    data++; // ignore alpha channel
            }
        }

        //Way more performant than GrayscaleByPixel and GrayscaleWithColorMatrix
        public static void GrayscaleWithLockBits(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = bitmapData.Stride / bitmapData.Width;
            IntPtr bitmapDataPointer = bitmapData.Scan0;
            byte[] rgbValues = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];
            Marshal.Copy(bitmapDataPointer, rgbValues, 0, rgbValues.Length);

            for (int i = 0; i < rgbValues.Length; i += bytesPerPixel)
            {
                //Colors are actually in BGRA order because computer CPUs read them with Little Endian, from back to front
                //Below method of grayscaling gives a better gray than to calculate the RGB average and set it as R, G and B
                //For 4 byte color formats, we will just ignore the alpha channel and leave it as it is
                //      B              G                  R                         B                       G                           R
                rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = (byte)(rgbValues[i] * 0.114F + rgbValues[i + 1] * 0.587F + rgbValues[i + 2] * 0.299F);
            }

            Marshal.Copy(rgbValues, 0, bitmapDataPointer, rgbValues.Length);
            bitmap.UnlockBits(bitmapData);
        }

        public static void GrayscaleWithColorMatrix(Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
                   });

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
                }
            }
        }

        //Thanks to Sathyaraj Palanisamy - https://stackoverflow.com/a/27397456
        public static void GrayscaleByPixel(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    byte gray = (byte)(.299 * color.R + .587 * color.G + .114 * color.B);
                    bitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
        }

        //Way more performant than DenoiseByPixel
        public static void DenoiseWithLockBits(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int bytesPerPixel = bitmapData.Stride / bitmapData.Width;
            byte[] rgbValues = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];
            IntPtr ptr = bitmapData.Scan0;
            Marshal.Copy(ptr, rgbValues, 0, rgbValues.Length);

            for (int i = 0; i < rgbValues.Length; i += bytesPerPixel)
            {
                //Colors are actually in BGRA order because computer CPUs read them with Little Endian, from back to front
                //For 4 byte color formats, we will just ignore the alpha channel and leave it as it is
                byte b = rgbValues[i];
                byte g = rgbValues[i + 1];
                byte r = rgbValues[i + 2];
                if (r < 162 && g < 162 && b < 162)
                    r = g = b = 0;
                else if (r > 162 && g > 162 && b > 162)
                    r = g = b = 255;
                rgbValues[i] = b;
                rgbValues[i + 1] = g;
                rgbValues[i + 2] = r;
            }

            Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);
            bitmap.UnlockBits(bitmapData);
        }

        //Thanks to Sathyaraj Palanisamy - https://stackoverflow.com/a/27397456
        public static void DenoiseByPixel(Bitmap bitmap)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel.R < 162 && pixel.G < 162 && pixel.B < 162)
                        bitmap.SetPixel(x, y, Color.Black);
                    else if (pixel.R > 162 && pixel.G > 162 && pixel.B > 162)
                        bitmap.SetPixel(x, y, Color.White);
                }
            }
        }
    }
}