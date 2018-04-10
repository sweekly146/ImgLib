using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImgLib.Visualise
{
    /// <summary>
    /// Visualisation functions for pairs of bitmaps.
    /// TODO: Investigate having one function with an enum parameter used to determine the visualisation. Will this have a performance impact? How would this work with RowBinaryDifference?
    /// </summary>
    public static class Visualiser
    {
        /// <summary>
        /// Find the real difference between two bitmaps. The real difference here means a visualisation where colors that are considered different are represented by the
        /// color specified by <param name="differentColor"></param> and colors that are considered the same are represented by the corresponding color in <param name="bmp1"></param>.
        /// </summary>
        /// <param name="bmp1">Source bitmap 1.</param>
        /// <param name="bmp2">Source bitmap 2.</param>
        /// <param name="colorChangeThreshold">Amount that each pixel component can vary by while still be considered the same.</param>
        /// <param name="differentColor">The color to use when the pixels are considered different.</param>
        /// <returns>Real difference bitmap.</returns>
        public static Bitmap RealDifference(Bitmap bmp1, Bitmap bmp2, int colorChangeThreshold, Color differentColor)
        {
            if (bmp1.Size != bmp2.Size)
            {
                throw new Exception("Source bitmaps are not the same size.");
            }
            if (bmp1.PixelFormat != bmp2.PixelFormat)
            {
                throw new Exception("Source bitmaps are not the same size.");
            }
            //Currently only supports Format24bppRgb and Format32bppArgb.
            if (bmp1.PixelFormat != PixelFormat.Format24bppRgb && bmp1.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("Source bitmaps using incompatible pixel formats. " +
                                    "Source bitmaps must be using either the Format24bppRgb or Format32bppArgb pixel format.");
            }

            Bitmap outBmp = new Bitmap(bmp1.Width, bmp2.Height, bmp1.PixelFormat);

            int bitsPerPixel = Bitmap.GetPixelFormatSize(bmp1.PixelFormat);
            int bytesPerPixel = bitsPerPixel / 8;                       
            
            Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bmp2Data = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData outBmpData = outBmp.LockBits(rect, ImageLockMode.WriteOnly, bmp1.PixelFormat);

            //Iterate through all of the rows of pixels in parallel.            
            Parallel.For(0, bmp1.Height, y =>
            {
                unsafe
                {                    
                    byte* bmp1CurrentLine = (byte*) bmp1Data.Scan0 + y * bmp1Data.Stride;
                    byte* bmp2CurrentLine = (byte*) bmp2Data.Scan0 + y * bmp2Data.Stride;
                    byte* outBmpCurrentLine = (byte*) outBmpData.Scan0 + y * outBmpData.Stride;

                    //Iterate through all of the pixels in the current row.
                    for (int x = 0; x < bmp1Data.Stride; x += bytesPerPixel)
                    {
                        //Assume pixels use BGRA ordering.
                        int bmp1B = bmp1CurrentLine[x];
                        int bmp1G = bmp1CurrentLine[x + 1];
                        int bmp1R = bmp1CurrentLine[x + 2];                        
                        int bmp2B = bmp2CurrentLine[x];
                        int bmp2G = bmp2CurrentLine[x + 1];
                        int bmp2R = bmp2CurrentLine[x + 2];

                        if (Math.Abs(bmp1B - bmp2B) > colorChangeThreshold ||
                            Math.Abs(bmp1G - bmp2G) > colorChangeThreshold ||
                            Math.Abs(bmp1R - bmp2R) > colorChangeThreshold)
                        {
                            outBmpCurrentLine[x] = differentColor.B;
                            outBmpCurrentLine[x + 1] = differentColor.G;
                            outBmpCurrentLine[x + 2] = differentColor.R;
                        }                        
                        else
                        {
                            outBmpCurrentLine[x] = bmp1CurrentLine[x];
                            outBmpCurrentLine[x + 1] = bmp1CurrentLine[x + 1];
                            outBmpCurrentLine[x + 2] = bmp1CurrentLine[x + 2];
                        }
                    }
                }
            });
            
            bmp1.UnlockBits(bmp1Data);
            bmp2.UnlockBits(bmp2Data);
            outBmp.UnlockBits(outBmpData);

            return outBmp;
        }

        /// <summary>
        /// Find the binary difference between two bitmaps. The binary difference here means a visualisation where colors that are considered different are represented by the
        /// color specified by <param name="differentColor"></param> and colors that are considered the same are represented by the color specified by <param name="sameColor"></param>.
        /// </summary>
        /// <param name="bmp1">Source bitmap 1.</param>
        /// <param name="bmp2">Source bitmap 2.</param>
        /// <param name="colorChangeThreshold">Amount that each pixel component can vary by while still be considered the same.</param>
        /// <param name="differentColor">Color to use when the pixels are considered different.</param>
        /// <param name="sameColor">Color to use when the pixels are considered the same.</param>
        /// <returns>Binary difference bitmap.</returns>
        public static Bitmap BinaryDifference(Bitmap bmp1, Bitmap bmp2, int colorChangeThreshold, Color differentColor, Color sameColor)
        {            
            if (bmp1.Size != bmp2.Size)
            {
                throw new Exception("Source bitmaps are not the same size.");
            }            
            if (bmp1.PixelFormat != bmp2.PixelFormat)
            {
                throw new Exception("Source bitmaps are not the same size.");
            }
            //Currently only supports Format24bppRgb and Format32bppArgb.
            if (bmp1.PixelFormat != PixelFormat.Format24bppRgb && bmp1.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("Source bitmaps using incompatible pixel formats. " +
                                    "Source bitmaps must be using either the Format24bppRgb or Format32bppArgb pixel format.");
            }

            Bitmap outBmp = new Bitmap(bmp1.Width, bmp2.Height, bmp1.PixelFormat);

            int bitsPerPixel = Bitmap.GetPixelFormatSize(bmp1.PixelFormat);
            int bytesPerPixel = bitsPerPixel / 8;                       
            
            Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bmp2Data = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData outBmpData = outBmp.LockBits(rect, ImageLockMode.WriteOnly, bmp1.PixelFormat);

            //Iterate through all of the rows of pixels in parallel.            
            Parallel.For(0, bmp1.Height, y =>
            {
                unsafe
                {                    
                    byte* bmp1CurrentLine = (byte*) bmp1Data.Scan0 + y * bmp1Data.Stride;
                    byte* bmp2CurrentLine = (byte*) bmp2Data.Scan0 + y * bmp2Data.Stride;
                    byte* outBmpCurrentLine = (byte*) outBmpData.Scan0 + y * outBmpData.Stride;

                    //Iterate through all of the pixels in the current row.
                    for (int x = 0; x < bmp1Data.Stride; x += bytesPerPixel)
                    {
                        //Assume pixels use BGRA ordering.
                        int bmp1B = bmp1CurrentLine[x];
                        int bmp1G = bmp1CurrentLine[x + 1];
                        int bmp1R = bmp1CurrentLine[x + 2];                        
                        int bmp2B = bmp2CurrentLine[x];
                        int bmp2G = bmp2CurrentLine[x + 1];
                        int bmp2R = bmp2CurrentLine[x + 2];
                        
                        if (Math.Abs(bmp1B - bmp2B) > colorChangeThreshold ||
                            Math.Abs(bmp1G - bmp2G) > colorChangeThreshold ||
                            Math.Abs(bmp1R - bmp2R) > colorChangeThreshold)
                        {
                            outBmpCurrentLine[x] = differentColor.B;
                            outBmpCurrentLine[x + 1] = differentColor.G;
                            outBmpCurrentLine[x + 2] = differentColor.R;
                        }                        
                        else
                        {
                            outBmpCurrentLine[x] = sameColor.B;
                            outBmpCurrentLine[x + 1] = sameColor.G;
                            outBmpCurrentLine[x + 2] = sameColor.R;
                        }
                    }
                }
            });
            
            bmp1.UnlockBits(bmp1Data);
            bmp2.UnlockBits(bmp2Data);
            outBmp.UnlockBits(outBmpData);

            return outBmp;
        }

        /// <summary>
        /// Find the row binary difference between two bitmaps. The row binary difference here means a visualisation where if the entire row of pixels is considered the same
        /// then the row of pixels is represented by the color <param name="rowSameColor"></param>.
        /// If the row of pixels is not considered the same then any colors that are not considered the same are represented by the color specified by <param name="differentColor"></param> 
        /// and colors that are considered the same are represented by the color specified by <param name="sameColor"></param>.
        /// </summary>
        /// <param name="bmp1">Source bitmap 1.</param>
        /// <param name="bmp2">Source bitmap 2.</param>        
        /// <param name="colorChangeThreshold">Amount that each pixel component can vary by while still be considered the same.</param>
        /// <param name="rowChangeThreshold">Amount of pixels in a row that can be different while still being considered a row that is the same.</param>
        /// <param name="differentColor">Color to use when the pixels are considered different.</param>
        /// <param name="rowSameColor">Color to use when every pixel in a row is the same.</param>
        /// <param name="sameColor">Color to use when the pixels are considered the same, but the entire row of pixels is not considered the same.</param>
        /// <returns>Row binary difference bitmap.</returns>
        public static Bitmap RowBinaryDifference(Bitmap bmp1, Bitmap bmp2, int colorChangeThreshold, int rowChangeThreshold, Color differentColor, Color rowSameColor, Color sameColor)
        {            
            if (bmp1.Size != bmp2.Size)
            {
                throw new Exception("Source bitmaps are not the same size.");
            }
            if (bmp1.PixelFormat != bmp2.PixelFormat)
            {
                throw new Exception("Source bitmaps are not the same size.");
            }
            //Currently only supports Format24bppRgb and Format32bppArgb.
            if (bmp1.PixelFormat != PixelFormat.Format24bppRgb && bmp1.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("Source bitmaps using incompatible pixel formats. " +
                                    "Source bitmaps must be using either the Format24bppRgb or Format32bppArgb pixel format.");
            }

            Bitmap outBmp = new Bitmap(bmp1.Width, bmp2.Height, bmp1.PixelFormat);

            int bitsPerPixel = Bitmap.GetPixelFormatSize(bmp1.PixelFormat);
            int bytesPerPixel = bitsPerPixel / 8;                       
            
            Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bmp2Data = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData outBmpData = outBmp.LockBits(rect, ImageLockMode.WriteOnly, bmp1.PixelFormat);

            //Iterate through all of the rows of pixels in parallel.
            Parallel.For(0, bmp1.Height, y =>
            {
                unsafe
                {                    
                    byte* bmp1CurrentLine = (byte*)bmp1Data.Scan0 + y * bmp1Data.Stride;
                    byte* bmp2CurrentLine = (byte*)bmp2Data.Scan0 + y * bmp2Data.Stride;
                    byte* outBmpCurrentLine = (byte*)outBmpData.Scan0 + y * outBmpData.Stride;

                    //Count the different pixels in the row.                    
                    int differentPixelsCount = 0;
                    for (int x = 0; x < bmp1Data.Stride; x += bytesPerPixel)
                    {
                        //Assume pixels use BGRA ordering.
                        int bmp1B = bmp1CurrentLine[x];
                        int bmp1G = bmp1CurrentLine[x + 1];
                        int bmp1R = bmp1CurrentLine[x + 2];                        
                        int bmp2B = bmp2CurrentLine[x];
                        int bmp2G = bmp2CurrentLine[x + 1];
                        int bmp2R = bmp2CurrentLine[x + 2];
                        
                        if (Math.Abs(bmp1B - bmp2B) > colorChangeThreshold ||
                            Math.Abs(bmp1G - bmp2G) > colorChangeThreshold ||
                            Math.Abs(bmp1R - bmp2R) > colorChangeThreshold)
                        {
                            differentPixelsCount++;                            
                        }
                    }                    
                    if (differentPixelsCount > rowChangeThreshold)
                    {
                        for (int x = 0; x < bmp1Data.Stride; x += bytesPerPixel)
                        {
                            //Assume pixels use BGRA ordering.
                            outBmpCurrentLine[x] = rowSameColor.B;
                            outBmpCurrentLine[x + 1] = rowSameColor.G;
                            outBmpCurrentLine[x + 2] = rowSameColor.R;
                        }
                    }                    
                    else
                    {                        
                        for (int x = 0; x < bmp1Data.Stride; x += bytesPerPixel)
                        {
                            //Assume pixels use BGRA ordering.
                            int bmp1B = bmp1CurrentLine[x];
                            int bmp1G = bmp1CurrentLine[x + 1];
                            int bmp1R = bmp1CurrentLine[x + 2];                            
                            int bmp2B = bmp2CurrentLine[x];
                            int bmp2G = bmp2CurrentLine[x + 1];
                            int bmp2R = bmp2CurrentLine[x + 2];

                            if (Math.Abs(bmp1B - bmp2B) > colorChangeThreshold ||
                                Math.Abs(bmp1G - bmp2G) > colorChangeThreshold ||
                                Math.Abs(bmp1R - bmp2R) > colorChangeThreshold)
                            {
                                outBmpCurrentLine[x] = differentColor.B;
                                outBmpCurrentLine[x + 1] = differentColor.G;
                                outBmpCurrentLine[x + 2] = differentColor.R;
                            }
                            else
                            {
                                outBmpCurrentLine[x] = sameColor.B;
                                outBmpCurrentLine[x + 1] = sameColor.G;
                                outBmpCurrentLine[x + 2] = sameColor.R;
                            }
                        }
                    }
                }
            });
            
            bmp1.UnlockBits(bmp1Data);
            bmp2.UnlockBits(bmp2Data);
            outBmp.UnlockBits(outBmpData);

            return outBmp;
        }
    }
}