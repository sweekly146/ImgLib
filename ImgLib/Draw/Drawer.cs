using ImgLib.Create;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImgLib.Draw
{
    /// <summary>
    /// Bitmap drawing functions. Name may change.
    /// </summary>
    public static class Drawer
    {
        public static Bitmap DrawBars(Color contrastColor, Color color, Size size, int startPoint, int endPoint, int lineWidth, List<Point> points)
        {
            if (points.Count == 0)
            {
                return null;
            }

            byte[] byteContrastColor = new byte[4];
            byteContrastColor[0] = contrastColor.B;
            byteContrastColor[1] = contrastColor.G;
            byteContrastColor[2] = contrastColor.R;
            byteContrastColor[3] = contrastColor.A;
            byte[] byteColor = new byte[4];
            byteColor[0] = color.B;
            byteColor[1] = color.G;
            byteColor[2] = color.R;
            byteColor[3] = color.A;

            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            unsafe
            {                
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                byte* scan0 = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                byte* columnStart = scan0 + (startPoint * stride);                
                const int bpp = 4;
                int bmpWidth = bmp.Width;
                int bmpHeight = bmp.Height;

                //Iterate through each bar to draw.
                for (int i = 0; i < points.Count; i++)
                {
                    int i2 = i;
                    Point point = points[i2];

                    //Draw each column in the bar.
                    for (int x = point.X; x < point.X + lineWidth && x < bmpWidth; x++)
                    {
                        //Don't draw bars if out of bounds.
                        if (x < 0)
                        {
                            continue;
                        }

                        byte* row = columnStart;
                        int xByte = bpp * x;
                        
                        for (int y = startPoint; y < startPoint + point.Y; y++, row += stride)
                        {
                            row[xByte] = byteContrastColor[0];
                            row[xByte + 1] = byteContrastColor[1];
                            row[xByte + 2] = byteContrastColor[2];
                            row[xByte + 3] = byteContrastColor[3];
                        }                        
                        for (int y = point.Y + startPoint; y < endPoint; y++, row += stride)
                        {
                            row[xByte] = byteColor[0];
                            row[xByte + 1] = byteColor[1];
                            row[xByte + 2] = byteColor[2];
                            row[xByte + 3] = byteColor[3];
                        }
                    }
                }
                
                bmp.UnlockBits(bmpData);
            }

            return bmp;
        }

        /// <summary>
        /// Darken a Bitmap by a given amount.
        /// </summary>
        /// <param name="bmp">Source frame.</param>
        /// <param name="darkenAmount">The amount to reduce each color channel by.</param>
        public static unsafe void Darken(Bitmap bmp, int darkenAmount)
        {
            if (darkenAmount <= 0)
            {
                return;
            }
            //Pixel format Format24bppRgb is only supported currently.
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("Bmp pixel format is not 24bpp rgb.");
            }

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* bmpScan0 = (byte*)bmpData.Scan0;
            int bmpWidth = bmp.Width;
            int stride = bmpData.Stride;

            Parallel.For(0, bmp.Height, y =>
            {
                byte* row = bmpScan0 + y * stride;

                for (int x = 0; x < bmpWidth; x++)
                {
                    *row = DrawHelper.Clamp(*row - darkenAmount);
                    row++;
                    *row = DrawHelper.Clamp(*row - darkenAmount);
                    row++;
                    *row = DrawHelper.Clamp(*row - darkenAmount);
                    row++;
                }
            });

            bmp.UnlockBits(bmpData);
        }

        public static Bitmap DrawLeftIndicator(int width, int height, int borderSize, Color color, Color borderColor)
        {
            Bitmap bmp = Creator.CreateColoredBitmap(width, height, Color.FromArgb(255, borderColor.R, borderColor.G, borderColor.B), PixelFormat.Format32bppArgb);
            Rectangle rectangle = new Rectangle(0, borderSize, width - borderSize, height - (2 * borderSize));
            FillRectangleWithColor(bmp, color, rectangle);

            return bmp;
        }

        public static Bitmap DrawRightIndicator(int width, int height, int borderSize, Color color, Color borderColor)
        {
            Bitmap bmp = Creator.CreateColoredBitmap(width, height, Color.FromArgb(255, borderColor.R, borderColor.G, borderColor.B), PixelFormat.Format32bppArgb);
            Rectangle rectangle = new Rectangle(borderSize, borderSize, width - borderSize, height - (2 * borderSize));
            FillRectangleWithColor(bmp, color, rectangle);

            return bmp;
        }

        /// <summary>
        /// Fills a rectangle inside of a bitmap with a specified color. Currently only works with bitmaps using a pixel format of either Format24bppRgb or Format32bppArgb.
        /// </summary>
        /// <param name="bmp">Bitmap to change the color of.</param>
        /// <param name="color">Color to fill the rectangle with.</param>
        /// <param name="rectangle">Rectangle inside the bitmap to fill with color.</param>
        public static unsafe void FillRectangleWithColor(Bitmap bmp, Color color, Rectangle rectangle)
        {
            unsafe
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int bpp = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int[] sameLineArr = new int[bmp.Height];
                byte* bmpScan0 = (byte*)bmpData.Scan0;
                int xStart = rectangle.X * bpp;
                int width = rectangle.Width;
                byte colorB = color.B;
                byte colorG = color.G;
                byte colorR = color.R;
                byte colorA = color.A;
                int start = Math.Max(rectangle.Y, 0);
                int end = Math.Min(rectangle.Y + rectangle.Height, bmp.Height);

                //Need to handle two cases where the bitmap is Format24bppRgb or Format32bppArgb.
                //Duplicate code because handling this nested in loops will cause degraded performance.
                if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Parallel.For(start, end, y =>
                    {
                        byte* bmpCurrentRow = bmpScan0 + y * bmpData.Stride + xStart;

                        for (int x = 0; x < width; x++)
                        {
                            *bmpCurrentRow++ = colorB;
                            *bmpCurrentRow++ = colorG;
                            *bmpCurrentRow++ = colorR;
                        }
                    });
                }
                if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    Parallel.For(start, end, y =>
                    {
                        byte* bmpCurrentRow = bmpScan0 + y * bmpData.Stride + xStart;

                        for (int x = 0; x < width; x++)
                        {
                            *bmpCurrentRow++ = colorB;
                            *bmpCurrentRow++ = colorG;
                            *bmpCurrentRow++ = colorR;
                            *bmpCurrentRow++ = colorA;
                        }
                    });
                }

                bmp.UnlockBits(bmpData);
            }
        }
    }
}