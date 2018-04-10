using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImgLib.Transform
{
    /// <summary>
    /// Bitmap transformation functions.
    /// </summary>
    public static class Transformer
    {
        /// <summary>
        /// Gets the transpose of a bitmap. Here a transpose operation means swapping the rows and columns.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Transposed bitmap.</returns>
        public static Bitmap Transpose(Bitmap bmp, int maxDegreeOfParallelism)
        {
            switch(bmp.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return TransposeFormat24bppRgb(bmp, maxDegreeOfParallelism);

                case PixelFormat.Format32bppArgb:
                    return TransposeFormat32bppArgb(bmp, maxDegreeOfParallelism);

                default:
                    throw new Exception("Invalid Pixel Format");
            }
        }

        /// <summary>
        /// Gets a transposed copy of a bitmap. Here a transpose operation means swapping the rows and columns.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>        
        /// <returns>Transposed bitmap.</returns>
        public static Bitmap Transpose(Bitmap bmp)
        {
            return Transpose(bmp, Environment.ProcessorCount);
        }

        /// <summary>
        /// Gets a transposed copy of a bitmap that has a Format24bppRgb Pixel Format. Here a transpose operation means swapping the rows and columns.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Transposed bitmap.</returns>
        private static unsafe Bitmap TransposeFormat24bppRgb(Bitmap inBmp, int maxDegreeOfParallelism)
        {
            Bitmap outBmp = new Bitmap(inBmp.Height, inBmp.Width, PixelFormat.Format24bppRgb);

            BitmapData inBmpData = inBmp.LockBits(new Rectangle(0, 0, inBmp.Width, inBmp.Height), ImageLockMode.ReadOnly, inBmp.PixelFormat);
            BitmapData outBmpData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, outBmp.PixelFormat);

            byte* inBmpDataScan0 = (byte*)inBmpData.Scan0;
            int inBmpDataStride = inBmpData.Stride;
            byte* outBmpDataScan0 = (byte*)outBmpData.Scan0;
            int outBmpDataStride = outBmpData.Stride;
            int bpp = 3;

            int srcLen = inBmpDataStride * inBmp.Height;
            byte* srcEnd = inBmpDataScan0 + srcLen;

            Parallel.For(0, outBmp.Height, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, outY =>
            {
                byte* dest = outBmpDataScan0 + outY * outBmpDataStride;
                byte* src = inBmpDataScan0 + outY * bpp;

                while (src < srcEnd)
                {
                    *dest++ = src[0];
                    *dest++ = src[1];
                    *dest++ = src[2];

                    src += inBmpDataStride;
                }
            });

            inBmp.UnlockBits(inBmpData);
            outBmp.UnlockBits(outBmpData);

            return outBmp;
        }

        /// <summary>
        /// Gets a transposed copy of a bitmap that has a Format32bppArgb Pixel Format. Here a transpose operation means swapping the rows and columns.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Transposed bitmap.</returns>
        private static unsafe Bitmap TransposeFormat32bppArgb(Bitmap inBmp, int maxDegreeOfParallelism)
        {
            Bitmap outBmp = new Bitmap(inBmp.Height, inBmp.Width, PixelFormat.Format32bppArgb);

            BitmapData inBmpData = inBmp.LockBits(new Rectangle(0, 0, inBmp.Width, inBmp.Height), ImageLockMode.ReadOnly, inBmp.PixelFormat);
            BitmapData outBmpData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, outBmp.PixelFormat);

            byte* inBmpDataScan0 = (byte*)inBmpData.Scan0;
            int inBmpDataStride = inBmpData.Stride;
            byte* outBmpDataScan0 = (byte*)outBmpData.Scan0;
            int outBmpDataStride = outBmpData.Stride;
            int bpp = 4;

            int srcLen = inBmpDataStride * inBmp.Height;
            byte* srcEnd = inBmpDataScan0 + srcLen;

            Parallel.For(0, outBmp.Height, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, outY =>
            {
                byte* dest = outBmpDataScan0 + outY * outBmpDataStride;
                byte* src = inBmpDataScan0 + outY * bpp;

                while(src < srcEnd)
                {
                    *dest++ = src[0];
                    *dest++ = src[1];
                    *dest++ = src[2];
                    *dest++ = src[3];

                    src += inBmpDataStride;
                }
            });

            inBmp.UnlockBits(inBmpData);
            outBmp.UnlockBits(outBmpData);

            return outBmp;
        }
    }
}