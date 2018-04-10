using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImgLib.Superimpose
{
    /// <summary>
    /// Functions for superimposing one bitmap onto another.
    /// TODO: Improve alpha blend speed when both bitmaps have an alpha channel.
    /// </summary>
    public static class Superimposer
    {
        /// <summary>
        /// Superimpose one bitmap onto another using GDI.
        /// </summary>
        /// <param name="backBmp">Bitmap to be drawn onto.</param>
        /// <param name="frontBmp">Bitmap to draw from.</param>
        /// <param name="transparent">Whether or not transparency is desired.</param>        
        /// <param name="point">Point on the back bitmap to copy the front bitmap.</param>     
        public static void SuperimposeGdi(Bitmap backBmp, Bitmap frontBmp, bool transparent, Point point)
        {            
            using (Graphics g = Graphics.FromImage(backBmp))
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.None;

                if (transparent)
                {
                    g.CompositingMode = CompositingMode.SourceOver;
                }
                else
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                }
                
                g.DrawImage(frontBmp, point);
            }
        }

        /// <summary>
        /// Superimposes one bitmap onto another.
        /// </summary>
        /// <param name="backBmp">Bitmap to be drawn onto.</param>
        /// <param name="frontBmp">Bitmap to draw from.</param>
        /// <param name="transparent">Whether or not transparency is desired.</param>        
        /// <param name="point">Point on the back bitmap to copy the front bitmap.</param>        
        public static void Superimpose(Bitmap backBmp, Bitmap frontBmp, bool transparent, Point point)
        {
            BitmapData backBmpData = backBmp.LockBits(new Rectangle(0, 0, backBmp.Width, backBmp.Height), ImageLockMode.ReadWrite, backBmp.PixelFormat);

            Superimpose(backBmpData, frontBmp, transparent, point);

            backBmp.UnlockBits(backBmpData);
        }

        /// <summary>
        /// Superimposes one bitmap onto another.
        /// </summary>
        /// <param name="backBmpData">BitmapData from the bitmap to be drawn onto.</param>
        /// <param name="frontBmp">Bitmap to draw from.</param>
        /// <param name="transparent">Whether or not transparency is desired.</param>        
        /// <param name="point">Point on the back bitmap to copy the front bitmap.</param>        
        public static void Superimpose(BitmapData backBmpData, Bitmap frontBmp, bool transparent, Point point)
        {
            //Currently only supports Format24bppRgb and Format32bppArgb.
            if((backBmpData.PixelFormat != PixelFormat.Format24bppRgb && backBmpData.PixelFormat != PixelFormat.Format32bppArgb) ||
               (frontBmp.PixelFormat != PixelFormat.Format24bppRgb && frontBmp.PixelFormat != PixelFormat.Format32bppArgb))
            {
                throw new Exception("Currently only supports Format24bppRgb and Format32bppArgb.");
            }          
            //If the point is not contained in the back bitmap then we return doing nothing as that is implicitly performing the out of bounds superimposition.
            if(backBmpData.Width <= point.X || backBmpData.Height <= point.Y)
            {
                return;
            }
            
            Size size = SuperimposerHelper.GetFrontBmpSize(frontBmp, point, backBmpData.Width, backBmpData.Height);

            BitmapData frontBmpData = frontBmp.LockBits(new Rectangle(0, 0, frontBmp.Width, frontBmp.Height), ImageLockMode.ReadOnly, frontBmp.PixelFormat);

            if (transparent && frontBmp.PixelFormat == PixelFormat.Format32bppArgb)
            {
                Blend(backBmpData, frontBmpData, point, size);
            }
            else
            {
                Copy(backBmpData, frontBmpData, point, size);                
            }

            frontBmp.UnlockBits(frontBmpData);
        }

        /// <summary>
        /// Copies the pixels from one bitmap onto another.
        /// </summary>
        /// <param name="backBmpData">BitmapData from the bitmap to be drawn onto.</param>
        /// <param name="frontBmpData">BitmapData from the bitmap to draw from.</param>
        /// <param name="point">Point on the back bitmap to copy the front bitmap.</param>
        /// <param name="size">Size of the subset of pixels to use from the front bitmap.</param>
        private static void Copy(BitmapData backBmpData, BitmapData frontBmpData, Point point, Size size)
        {
            unsafe
            {       
                //Bpp of each bitmap may be different.
                int backBpp = Bitmap.GetPixelFormatSize(backBmpData.PixelFormat) / 8;
                int frontBpp = Bitmap.GetPixelFormatSize(frontBmpData.PixelFormat) / 8;
                int minBpp = Math.Min(backBpp, frontBpp);

                int pointX = point.X;
                int pointY = point.Y;
                int sizeHeight = size.Height;
                int frontStride = frontBmpData.Stride;
                int frontSizeStride = size.Width * frontBpp;
                byte* frontBmpDataScan0 = (byte*)frontBmpData.Scan0;
                int backStride = backBmpData.Stride;
                byte* backBmpDataScan0 = (byte*)backBmpData.Scan0;

                //Iterate through each row of pixels in parallel.
                Parallel.For(0, sizeHeight, y =>
                {
                    byte* backRow = backBmpDataScan0 + (y + pointY) * backStride;
                    byte* frontRow = frontBmpDataScan0 + y * frontStride;

                    int backRowByte = pointX * backBpp;
                    int frontRowByte = 0;

                    //Iterate through the pixels in the rows. Should only need to check one condition as the same amount of pixels should be traversed in both bitmaps.
                    for (; frontRowByte < frontSizeStride; backRowByte += backBpp, frontRowByte += frontBpp)
                    {
                        for(int i = 0; i < minBpp; i++)
                        {
                            backRow[backRowByte + i] = frontRow[frontRowByte + i];
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Blends one bitmap into another using alpha blending.
        /// </summary>
        /// <param name="backBmpData">BitmapData from the bitmap to be drawn onto.</param>
        /// <param name="frontBmpData">BitmapData from the bitmap to draw from.</param>
        /// <param name="point">Point on the back bitmap to copy the front bitmap.</param>
        /// <param name="size">Size of the subset of pixels to use from the front bitmap.</param>
        private static void Blend(BitmapData backBmpData, BitmapData frontBmpData, Point point, Size size)
        {
            unsafe
            {
                //Bpp of each bitmap may be different.
                int backBpp = Bitmap.GetPixelFormatSize(backBmpData.PixelFormat) / 8;
                int frontBpp = Bitmap.GetPixelFormatSize(frontBmpData.PixelFormat) / 8;

                int pointX = point.X;
                int pointY = point.Y;
                int sizeHeight = size.Height;
                int frontStride = frontBmpData.Stride;
                int frontSizeStride = size.Width * frontBpp;
                byte* frontBmpDataScan0 = (byte*)frontBmpData.Scan0;
                int backStride = backBmpData.Stride;
                byte* backBmpDataScan0 = (byte*)backBmpData.Scan0;

                //Need to handle two cases where the back bitmap is Format24bppRgb or Format32bppArgb.
                //Lots of duplicate code because handling this nested in loops will cause degraded performance.
                if (backBmpData.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    //Iterate through each row of pixels in parallel.
                    Parallel.For(0, sizeHeight, y =>
                    {
                        byte* backRow = backBmpDataScan0 + ((y + pointY) * backStride);
                        byte* frontRow = frontBmpDataScan0 + (y * frontStride);

                        int backRowByte = pointX * backBpp;
                        int frontRowByte = 0;

                        //Iterate through the pixels in the rows. Should only need to check one condition as the same amount of pixels should be traversed in both bitmaps.
                        for (; frontRowByte < frontSizeStride; backRowByte += backBpp, frontRowByte += frontBpp)
                        {
                            int alpha = frontRow[frontRowByte + 3] + 1;
                            int invAlpha = 256 - frontRow[frontRowByte + 3];
                            backRow[backRowByte] = (byte)((alpha * frontRow[frontRowByte] + invAlpha * backRow[backRowByte]) >> 8);
                            backRow[backRowByte + 1] = (byte)((alpha * frontRow[frontRowByte + 1] + invAlpha * backRow[backRowByte + 1]) >> 8);
                            backRow[backRowByte + 2] = (byte)((alpha * frontRow[frontRowByte + 2] + invAlpha * backRow[backRowByte + 2]) >> 8);
                        }
                    });
                }
                if (backBmpData.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    //Iterate through each row of pixels in parallel.
                    Parallel.For(0, sizeHeight, y =>
                    {
                        byte* backRow = backBmpDataScan0 + ((y + pointY) * backStride);
                        byte* frontRow = frontBmpDataScan0 + (y * frontStride);

                        int backRowByte = pointX * backBpp;
                        int frontRowByte = 0;

                        //Iterate through the pixels in the rows. Should only need to check one condition as the same amount of pixels should be traversed in both bitmaps.
                        for (; frontRowByte < frontSizeStride; backRowByte += backBpp, frontRowByte += frontBpp)
                        {
                            int frontAlphaByte = frontRow[frontRowByte + 3];
                            int inverseFrontAlphaByte = frontRow[frontRowByte + 3];
                            int backAlphaByte = (backRow[backRowByte + 3]);

                            float backAlpha = backAlphaByte / 255f;
                            float frontAlpha = frontAlphaByte / 255f;
                            float inverseFrontAlpha = 1 - frontAlpha;

                            float blendedAlpha = (backAlpha + (1 - backAlpha) * frontAlpha);

                            backRow[backRowByte] = SuperimposerHelper.Clamp((frontRow[frontRowByte] * frontAlpha + backRow[backRowByte] * backAlpha * inverseFrontAlpha) / blendedAlpha);
                            backRow[backRowByte + 1] = SuperimposerHelper.Clamp((frontRow[frontRowByte + 1] * frontAlpha + backRow[backRowByte + 1] * backAlpha * inverseFrontAlpha) / blendedAlpha);
                            backRow[backRowByte + 2] = SuperimposerHelper.Clamp((frontRow[frontRowByte + 2] * frontAlpha + backRow[backRowByte + 2] * backAlpha * inverseFrontAlpha) / blendedAlpha);
                            backRow[backRowByte + 3] = SuperimposerHelper.Clamp(255 * blendedAlpha);
                        }
                    });
                }
            }
        }
    }
}