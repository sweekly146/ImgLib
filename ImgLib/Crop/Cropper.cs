using ImgLib.Transform;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImgLib.Crop
{
    /// <summary>
    /// Bitmap cropping functions.
    /// </summary>
    public static class Cropper
    {
        /// <summary>        
        /// Get a copy of the specified bitmap and crop all adjacent fully transparent lines of pixels starting from each boundary.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="dispose">Whether or not to dispose of the source bitmap.</param>
        /// <returns>Cropped bitmap.</returns>
        public static Bitmap GetCroppedBitmap(Bitmap bmp, bool dispose)
        {            
            Bitmap outBmp;
            int leftLine = FindFirstNonTransparentLineLeft(bmp);
            int rightLine = FindFirstNonTransparentLineRight(bmp);                      
            int topLine = FindFirstNonTransparentLineTop(bmp);
            int bottomLine = FindFirstNonTransparentLineBottom(bmp);

            //If the bitmap is fully transparent then we will return a clone of the bitmap.
            if (rightLine == -1 || leftLine == -1 || topLine == -1 || bottomLine == -1)
            {
                outBmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);

                if (dispose)
                {
                    bmp.Dispose();
                }

                return outBmp;
            }

            //Calculate the new width and height for the bitmap. We need to add one to the right and bottom as the width and height start from one not zero.
            int width = rightLine + 1 - leftLine;
            int height = bottomLine + 1 - topLine;
            
            outBmp = bmp.Clone(new Rectangle(leftLine, topLine, width, height), bmp.PixelFormat);

            if (dispose)
            {
                bmp.Dispose();
            }

            return outBmp;
        }

        /// <summary>
        /// Get a copy of the specified bitmap and crop all adjacent fully transparent lines of pixels starting from the leftmost boundary.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="dispose">Whether or not to dispose of the source bitmap.</param>
        /// <returns>Cropped bitmap.</returns>
        public static Bitmap GetLeftCroppedBitmap(Bitmap bmp, bool dispose)
        {            
            Bitmap outBmp;
            int leftLine = FindFirstNonTransparentLineLeft(bmp);

            //If the bitmap is fully transparent then we will return a clone of the bitmap.
            if (leftLine == -1)
            {
                outBmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);

                if (dispose)
                {
                    bmp.Dispose();
                }

                return outBmp;
            }
            
            int width = bmp.Width - leftLine;         
            
            outBmp = bmp.Clone(new Rectangle(0, 0, width, bmp.Height), bmp.PixelFormat);

            if (dispose)
            {
                bmp.Dispose();
            }

            return outBmp;
        }

        /// <summary>
        /// Get a copy of the specified bitmap and crop all adjacent fully transparent lines of pixels starting from the rightmost boundary.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="dispose">Whether or not to dispose of the source bitmap.</param>
        /// <returns>Cropped bitmap.</returns>
        public static Bitmap GetRightCroppedBitmap(Bitmap bmp, bool dispose)
        {            
            Bitmap outBmp;
            int rightLine = FindFirstNonTransparentLineRight(bmp);

            //If the bitmap is fully transparent then we will return a clone of the bitmap.
            if (rightLine == -1)
            {
                outBmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);

                if (dispose)
                {
                    bmp.Dispose();
                }

                return outBmp;
            }

            //Calculate the new width for the bitmap. We need to add one to the right as the width starts from one not zero.
            int width = rightLine + 1;     
            
            outBmp = bmp.Clone(new Rectangle(0, 0, width, bmp.Height), bmp.PixelFormat);

            if (dispose)
            {
                bmp.Dispose();
            }

            return outBmp;
        }

        /// <summary>
        /// Search through all vertical lines of pixels in the bitmap starting at the very left looking for the first line to contain a non-zero alpha value.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <returns>X coordinate of vertical line of pixels that contains a non-zero alpha value.</returns>
        private static unsafe int FindFirstNonTransparentLineLeft(Bitmap bmp)
        {            
            //Transpose the bitmap. Since the columns are now rows then a leftmost search will now be a topmost search.
            Bitmap transposedBmp = Transformer.Transpose(bmp, Environment.ProcessorCount);
            int x = FindFirstNonTransparentLineTop(transposedBmp);

            transposedBmp.Dispose();

            return x;
        }

        /// <summary>
        /// Search through all vertical lines of pixels in the bitmap starting at the very right looking for the first line to contain a non-zero alpha value.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <returns>X coordinate of vertical line of pixels that contains a non-zero alpha value.</returns>
        private static unsafe int FindFirstNonTransparentLineRight(Bitmap bmp)
        {
            //Transpose the bitmap. Since the columns are now rows then a rightmost search will now be a bottommost search.
            Bitmap transposedBmp = Transformer.Transpose(bmp, Environment.ProcessorCount);
            int x = FindFirstNonTransparentLineBottom(transposedBmp);

            transposedBmp.Dispose();

            return x;
        }

        /// <summary>
        /// Search through all horizontal lines of pixels in the bitmap starting at the very top looking for the first line to contain a non-zero alpha value.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <returns>Y coordinate of horizontal line of pixels that contains a non-zero alpha value.</returns>
        private static unsafe int FindFirstNonTransparentLineTop(Bitmap bmp)
        {
            //Currently only supports Format32bppArgb pixel format.
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("Pixel format must be 32bitargb for this operation.");
            }

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int bpp = 4;

            for (int y = 0; y < bmpData.Height; y++)
            {                
                byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;

                //Start on 3 as that is the byte that contains the alpha channel.
                for (int x = 3; x < bmpData.Stride; x += bpp)
                {
                    if (row[x] != 0)
                    {
                        bmp.UnlockBits(bmpData);
                        return y;
                    }
                }
            }

            bmp.UnlockBits(bmpData);

            return -1;
        }

        /// <summary>
        /// Search through all horizontal lines of pixels in the bitmap starting at the very bottom looking for the first line to contain a non-zero alpha value.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <returns>Y coordinate of horizontal line of pixels that contains a non-zero alpha value.</returns>
        private static unsafe int FindFirstNonTransparentLineBottom(Bitmap bmp)
        {
            //Currently only supports Format32bppArgb pixel format.
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("Pixel format must be 32bitargb for this operation.");
            }

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int bpp = 4;


            for (int y = bmpData.Height - 1; y >= 0; y--)
            {
                byte* row = (byte*)bmpData.Scan0 + y * bmpData.Stride;

                //Start on 3 as that is the byte that contains the alpha channel.
                for (int x = 3; x < bmpData.Stride; x += bpp)
                {
                    if (row[x] != 0)
                    {
                        bmp.UnlockBits(bmpData);
                        return y;
                    }
                }
            }

            bmp.UnlockBits(bmpData);

            return -1;
        }
    }
}