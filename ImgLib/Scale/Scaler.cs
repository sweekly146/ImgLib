using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImgLib.Scale
{
    /// <summary>
    /// Bitmap scaling functions.
    /// TODO: Implement correct downsampling. Currently downsampling acts similarly to nearest neighbour downsampling even for bilinear and bicubic scaling.
    /// </summary>
    public static class Scaler
    {
        /// <summary>
        /// Interpolation modes used for image scaling. 
        /// </summary>
        public enum InterpolationMode
        {
            /// <summary>
            /// Pixels from the output bitmap are set to the nearest corresponding pixel from the input bitmap.
            /// </summary>
            NearestNeighbour,

            /// <summary>
            /// Pixels from the output bitmap are linearly interpolated from the four closest corresponding pixels from the input bitmap.
            /// </summary>
            Bilinear,

            /// <summary>
            /// Pixels from the output bitmap are interpolated using a cubic function from the four closest corresponding pixels on each axis (scaling is separated) from the input bitmap. 
            /// The type of cubic interpolation used here is Catmull-Rom.
            /// </summary>
            Bicubic
        }

        /// <summary>
        /// Scale a bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to be scaled.</param>
        /// <param name="size">Size to scale to.</param>
        /// <param name="interpolationMode">Interpolation mode used to scale the image.</param>
        /// <returns>Scaled bitmap.</returns>
        public static Bitmap Scale(Bitmap bmp, Size size, InterpolationMode interpolationMode)
        {
            return Scale(bmp, size, interpolationMode, Environment.ProcessorCount);
        }

        /// <summary>
        /// Scale a bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to be scaled.</param>
        /// <param name="size">Size to scale to.</param>
        /// <param name="interpolationMode">Interpolation mode used to scale the image.</param>        
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Scaled bitmap.</returns>
        public static Bitmap Scale(Bitmap inBmp, Size newSize, InterpolationMode interpolationMode, int maxDegreeOfParallelism)
        {
            switch (interpolationMode)
            {
                case InterpolationMode.NearestNeighbour:
                    return NearestNeighbour(inBmp, newSize, maxDegreeOfParallelism);

                case InterpolationMode.Bilinear:
                    return Bilinear(inBmp, newSize, maxDegreeOfParallelism);

                case InterpolationMode.Bicubic:
                    return Bicubic(inBmp, newSize, maxDegreeOfParallelism);

                default:
                    return NearestNeighbour(inBmp, newSize, maxDegreeOfParallelism);
            }
        }

        #region NearestNeighbour

        /// <summary>
        /// Scale a bitmap using Nearest Neighbour interpolation.
        /// </summary>
        /// <param name="inBmp">Bitmap to be scaled.</param>
        /// <param name="size">Size to scale to.</param>        
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Scaled bitmap.</returns>
        private static unsafe Bitmap NearestNeighbour(Bitmap inBmp, Size size, int maxDegreeOfParallelism)
        {
            if (inBmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("PixelFormat incorrect. Must be Format24bppRgb.");
            }

            Bitmap outBmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            BitmapData inData = inBmp.LockBits(new Rectangle(0, 0, inBmp.Width, inBmp.Height), ImageLockMode.ReadOnly, inBmp.PixelFormat);
            BitmapData outData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.ReadWrite, outBmp.PixelFormat);

            int bpp = 3;
            float xRatio = inBmp.Width / (float)outBmp.Width;
            float yRatio = inBmp.Height / (float)outBmp.Height;

            byte* outScan0 = (byte*)outData.Scan0;
            int outStride = outData.Stride;
            int outWidth = outBmp.Width;
            byte* inScan0 = (byte*)inData.Scan0;
            int inStride = inData.Stride;           

            Parallel.For(0, outBmp.Height, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, outY =>
            {
                float centerY = (outY + 0.5f) * yRatio;

                byte* outRow = outScan0 + ((int)outY * outStride);
                byte* inRow = inScan0 + ((int)centerY * inStride);

                for (float outX = 0.5f; outX < outWidth; outX++)
                {
                    float centerX = outX * xRatio;

                    int inCol = (int)centerX * bpp;

                    *outRow++ = inRow[inCol];
                    *outRow++ = inRow[inCol + 1];
                    *outRow++ = inRow[inCol + 2];
                }
            });

            outBmp.UnlockBits(outData);
            inBmp.UnlockBits(inData);

            return outBmp;
        }

        #endregion

        #region Bilinear     

        /// <summary>
        /// Scale a bitmap using Bilinear interpolation.
        /// </summary>
        /// <param name="inBmp">Bitmap to be scaled.</param>
        /// <param name="size">Size to scale to.</param>        
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Scaled bitmap.</returns>
        private static unsafe Bitmap Bilinear(Bitmap inBmp, Size size, int maxDegreeOfParallelism)
        {
            if (inBmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("PixelFormat incorrect. Must be Format24bppRgb.");
            }

            Bitmap outBmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData inBmpData = inBmp.LockBits(new Rectangle(0, 0, inBmp.Width, inBmp.Height), ImageLockMode.ReadOnly, inBmp.PixelFormat);
                BitmapData outBmpData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.ReadWrite, outBmp.PixelFormat);

                int bpp = 3;

                float xRatio = inBmp.Width / (float)outBmp.Width;
                float yRatio = inBmp.Height / (float)outBmp.Height;

                int inBmpWidth = inBmp.Width;
                int inBmpHeight = inBmp.Height;
                int lastInPixelX = inBmpWidth - 1;
                int lastInPixelY = inBmpHeight - 1;
                int outBmpWidth = outBmp.Width;
                int outBmpHeight = outBmp.Height;
                int outBmpDataStride = outBmpData.Stride;
                int inBmpDataStride = inBmpData.Stride;
                byte* outBmpDataScan0 = (byte*)outBmpData.Scan0;
                byte* inBmpDataScan0 = (byte*)inBmpData.Scan0;

                Parallel.For(0, outBmp.Height, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, outY =>
                {
                    float centerY = (outY + 0.5f) * yRatio;

                    int inY1 = (int)centerY;
                    float y1Dis = centerY - (inY1 + 0.5f);
                    int inY2;
                    if (y1Dis <= 0)
                    {
                        inY2 = Math.Max(inY1 - 1, 0);
                    }
                    else
                    {
                        inY2 = Math.Min(inY1 + 1, lastInPixelY);
                    }

                    byte* outRow = outBmpDataScan0 + (outY * outBmpDataStride);
                    byte* inRow1 = inBmpDataScan0 + (inY1 * inBmpDataStride);
                    byte* inRow2 = inBmpDataScan0 + (inY2 * inBmpDataStride);

                    y1Dis = Math.Abs(y1Dis);
                    float y2Dis = 1 - y1Dis;
                    float y1Weight = 1 - y1Dis;
                    float y2Weight = 1 - y1Weight;

                    for (float outX = 0.5f; outX < outBmpWidth; outX++)
                    {
                        float centerX = outX * xRatio;

                        int inX1 = (int)centerX;
                        float inX1Dis = centerX - (inX1 + 0.5f);
                        int inX2;
                        if (inX1Dis <= 0)
                        {
                            inX2 = Math.Max(inX1 - 1, 0);
                        }
                        else
                        {
                            inX2 = Math.Min(inX1 + 1, lastInPixelX);
                        }

                        int inCol1B = inX1 * bpp;
                        int inCol1G = inCol1B + 1;
                        int inCol1R = inCol1B + 2;
                        int inCol2B = inX2 * bpp;
                        int inCol2G = inCol2B + 1;
                        int inCol2R = inCol2B + 2;

                        byte b1 = inRow1[inCol1B];
                        byte g1 = inRow1[inCol1G];
                        byte r1 = inRow1[inCol1R];
                        byte b2 = inRow1[inCol2B];
                        byte g2 = inRow1[inCol2G];
                        byte r2 = inRow1[inCol2R];
                        byte b3 = inRow2[inCol1B];
                        byte g3 = inRow2[inCol1G];
                        byte r3 = inRow2[inCol1R];
                        byte b4 = inRow2[inCol2B];
                        byte g4 = inRow2[inCol2G];
                        byte r4 = inRow2[inCol2R];

                        inX1Dis = Math.Abs(inX1Dis);
                        float inX2Dis = 1 - inX1Dis;
                        float x1Weight = 1 - inX1Dis;
                        float x2Weight = 1 - x1Weight;

                        float weight1 = x1Weight * y1Weight;
                        float weight2 = x2Weight * y1Weight;
                        float weight3 = x1Weight * y2Weight;
                        float weight4 = x2Weight * y2Weight;

                        float weightNormaliser = 1f / (weight1 + weight2 + weight3 + weight4);

                        float outR = (r1 * weight1 + r2 * weight2 + r3 * weight3 + r4 * weight4) * weightNormaliser;
                        float outG = (g1 * weight1 + g2 * weight2 + g3 * weight3 + g4 * weight4) * weightNormaliser;
                        float outB = (b1 * weight1 + b2 * weight2 + b3 * weight3 + b4 * weight4) * weightNormaliser;

                        *outRow++ = ScaleHelper.Clamp(outB);
                        *outRow++ = ScaleHelper.Clamp(outG);
                        *outRow++ = ScaleHelper.Clamp(outR);
                    }
                });

                outBmp.UnlockBits(outBmpData);
                inBmp.UnlockBits(inBmpData);
            }

            return outBmp;
        }

        #endregion

        #region Bicubic

        /// <summary>
        /// Scale a bitmap using Bicubic interpolation.
        /// </summary>
        /// <param name="inBmp">Bitmap to be scaled.</param>
        /// <param name="size">Size to scale to.</param>        
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Scaled bitmap.</returns>
        private static unsafe Bitmap Bicubic(Bitmap inBmp, Size size, int maxDegreeOfParallelism)
        {
            Bitmap horizontalBmp = BicubicHorizontalUpscale(inBmp, size.Width, maxDegreeOfParallelism);            
            Bitmap verticalBmp = BicubicVerticalUpscale(horizontalBmp, size.Height, maxDegreeOfParallelism);            

            horizontalBmp.Dispose();            

            return verticalBmp;
        }

        /// <summary>
        /// Horizontal component of Bicubic interpolation.
        /// </summary>
        /// <param name="inBmp">Bitmap to be scaled.</param>
        /// <param name="outBmpWidth">Width to scale to.</param>        
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Scaled bitmap.</returns>
        private static Bitmap BicubicHorizontalUpscale(Bitmap inBmp, int outBmpWidth, int maxDegreeOfParallelism)
        {
            if (inBmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("PixelFormat incorrect. Must be Format24bppRgb.");
            }

            if (inBmp.Width == outBmpWidth)
            {
                return (Bitmap)inBmp.Clone();
            }

            Bitmap outBmp = new Bitmap(outBmpWidth, inBmp.Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData inData = inBmp.LockBits(new Rectangle(0, 0, inBmp.Width, inBmp.Height), ImageLockMode.ReadOnly, inBmp.PixelFormat);
                BitmapData outData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.WriteOnly, outBmp.PixelFormat);

                int bpp = Bitmap.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8;
                float xRatio = inBmp.Width / (float)outBmp.Width;
                int lastInPixelX = inBmp.Width - 1;                
                byte* outScan0 = (byte*)outData.Scan0;
                byte* inScan0 = (byte*)inData.Scan0;
                int outDataStride = outData.Stride;
                int intDataStride = inData.Stride;

                //Precalculate pixel component coordinates and weights.
                #region PreCalculate
                int[] cols = new int[outBmpWidth * 4];
                float[] weights = new float[outBmpWidth * 4];
                float x = 0.5f;
                for (int inPixel = 0; inPixel < weights.Length; x++, inPixel += 4)
                {
                    float centerX = x * xRatio;

                    int inX1 = (int)Math.Round(Math.Max(centerX - 2, 0));
                    int inX2 = Math.Min(inX1 + 1, lastInPixelX);
                    int inX3 = Math.Min(inX1 + 2, lastInPixelX);
                    int inX4 = Math.Min(inX1 + 3, lastInPixelX);

                    int col1 = inX1 * bpp;
                    int col2 = inX2 * bpp;
                    int col3 = inX3 * bpp;
                    int col4 = inX4 * bpp;

                    cols[inPixel] = col1;
                    cols[inPixel + 1] = col2;
                    cols[inPixel + 2] = col3;
                    cols[inPixel + 3] = col4;

                    //Calculate weights.
                    float distX1 = Math.Abs((inX1 + 0.5f) - centerX);
                    float distX2 = Math.Abs((inX2 + 0.5f) - centerX);
                    float distX3 = Math.Abs((inX3 + 0.5f) - centerX);
                    float distX4 = Math.Abs((inX4 + 0.5f) - centerX);                    
                    float weight1 = CubicKernel(distX1);
                    float weight2 = CubicKernel(distX2);
                    float weight3 = CubicKernel(distX3);
                    float weight4 = CubicKernel(distX4);
                    if (inX1 == inX2)
                    {
                        weight2 = 0;
                    }
                    if (inX2 == inX3)
                    {
                        weight3 = 0;
                    }
                    if (inX3 == inX4)
                    {
                        weight4 = 0;
                    }
                    
                    //Normalise weights and add them to the array.
                    float weightNormaliser = 1f / (weight1 + weight2 + weight3 + weight4);
                    weights[inPixel] = weight1 * weightNormaliser;
                    weights[inPixel + 1] = weight2 * weightNormaliser;
                    weights[inPixel + 2] = weight3 * weightNormaliser;
                    weights[inPixel + 3] = weight4 * weightNormaliser;

                }
                #endregion

                Parallel.For(0, outBmp.Height, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, y =>
                {
                    byte* outRow = outScan0 + (y * outDataStride);
                    byte* inRow = inScan0 + (y * intDataStride);

                    for (int pixel = 0; pixel < cols.Length; pixel += 4)
                    {
                        float inB1 = inRow[cols[pixel]];
                        float inG1 = inRow[cols[pixel] + 1];
                        float inR1 = inRow[cols[pixel] + 2];
                        float inB2 = inRow[cols[pixel + 1]];
                        float inG2 = inRow[cols[pixel + 1] + 1];
                        float inR2 = inRow[cols[pixel + 1] + 2];
                        float inB3 = inRow[cols[pixel + 2]];
                        float inG3 = inRow[cols[pixel + 2] + 1];
                        float inR3 = inRow[cols[pixel + 2] + 2];
                        float inB4 = inRow[cols[pixel + 3]];
                        float inG4 = inRow[cols[pixel + 3] + 1];
                        float inR4 = inRow[cols[pixel + 3] + 2];

                        float weight1 = weights[pixel];
                        float weight2 = weights[pixel + 1];
                        float weight3 = weights[pixel + 2];
                        float weight4 = weights[pixel + 3];

                        float outB = inB1 * weight1 + inB2 * weight2 + inB3 * weight3 + inB4 * weight4;
                        float outG = inG1 * weight1 + inG2 * weight2 + inG3 * weight3 + inG4 * weight4;
                        float outR = inR1 * weight1 + inR2 * weight2 + inR3 * weight3 + inR4 * weight4;

                        *outRow++ = ScaleHelper.Clamp(outB);
                        *outRow++ = ScaleHelper.Clamp(outG);
                        *outRow++ = ScaleHelper.Clamp(outR);
                    }
                });

                outBmp.UnlockBits(outData);
                inBmp.UnlockBits(inData);
            }

            return outBmp;
        }

        /// <summary>
        /// Vertical component of Bicubic interpolation.
        /// </summary>
        /// <param name="inBmp">Bitmap to be scaled.</param>
        /// <param name="outBmpHeight">Height to scale to.</param>        
        /// <param name="maxDegreeOfParallelism">How many threads should be used.</param>
        /// <returns>Scaled bitmap.</returns>
        private static Bitmap BicubicVerticalUpscale(Bitmap inBmp, int outBmpHeight, int maxDegreeOfParallelism)
        {
            if (inBmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("PixelFormat incorrect. Must be Format24bppRgb.");
            }
            if (inBmp.Height == outBmpHeight)
            {
                return (Bitmap)inBmp.Clone();
            }

            Bitmap outBmp = new Bitmap(inBmp.Width, outBmpHeight, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData inData = inBmp.LockBits(new Rectangle(0, 0, inBmp.Width, inBmp.Height), ImageLockMode.ReadOnly, inBmp.PixelFormat);
                BitmapData outData = outBmp.LockBits(new Rectangle(0, 0, outBmp.Width, outBmp.Height), ImageLockMode.ReadWrite, outBmp.PixelFormat);

                int bpp = Bitmap.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8;
                float yRatio = inBmp.Height / (float)outBmp.Height;
                int inBmpHeight = inBmp.Height;
                int lastInPixelY = inBmp.Height - 1;
                int outBmpWidth = outBmp.Width;
                byte* outBmpDataScan0 = (byte*)outData.Scan0;
                byte* inBmpDataScan0 = (byte*)inData.Scan0;
                int outDataStride = outData.Stride;
                int intDataStride = inData.Stride;

                Parallel.For(0, outBmp.Height, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, outY =>
                {                
                    float centerY = (outY + 0.5f) * yRatio;

                    int inY1 = (int)Math.Round(Math.Max(centerY - 2, 0));
                    int inY2 = Math.Min(inY1 + 1, lastInPixelY);
                    int inY3 = Math.Min(inY1 + 2, lastInPixelY);
                    int inY4 = Math.Min(inY1 + 3, lastInPixelY);

                    byte* outRow = outBmpDataScan0 + (outY * outDataStride);
                    byte* inRow1 = inBmpDataScan0 + (inY1 * intDataStride);
                    byte* inRow2 = inBmpDataScan0 + (inY2 * intDataStride);
                    byte* inRow3 = inBmpDataScan0 + (inY3 * intDataStride);
                    byte* inRow4 = inBmpDataScan0 + (inY4 * intDataStride);

                    float distY1 = Math.Abs((inY1 + 0.5f) - centerY);
                    float distY2 = Math.Abs((inY2 + 0.5f) - centerY);
                    float distY3 = Math.Abs((inY3 + 0.5f) - centerY);
                    float distY4 = Math.Abs((inY4 + 0.5f) - centerY);
                    float weight1 = CubicKernel(distY1);
                    float weight2 = CubicKernel(distY2);
                    float weight3 = CubicKernel(distY3);
                    float weight4 = CubicKernel(distY4);
                    if (inY1 == inY2)
                    {
                        weight2 = 0;
                    }
                    if (inY2 == inY3)
                    {
                        weight3 = 0;
                    }
                    if (inY3 == inY4)
                    {
                        weight4 = 0;
                    }
                    
                    //Normalise weights.
                    float weightNormaliser = 1f / (weight1 + weight2 + weight3 + weight4);
                    weight1 *= weightNormaliser;
                    weight2 *= weightNormaliser;
                    weight3 *= weightNormaliser;
                    weight4 *= weightNormaliser;

                    for (int x = 0; x < outBmpWidth; x++)
                    {
                        float outB = *inRow1++ * weight1 + *inRow2++ * weight2 + *inRow3++ * weight3 + *inRow4++ * weight4;
                        float outG = *inRow1++ * weight1 + *inRow2++ * weight2 + *inRow3++ * weight3 + *inRow4++ * weight4;
                        float outR = *inRow1++ * weight1 + *inRow2++ * weight2 + *inRow3++ * weight3 + *inRow4++ * weight4;

                        *outRow++ = ScaleHelper.Clamp(outB);
                        *outRow++ = ScaleHelper.Clamp(outG);
                        *outRow++ = ScaleHelper.Clamp(outR);
                    }
                });

                outBmp.UnlockBits(outData);
                inBmp.UnlockBits(inData);
            }

            return outBmp;
        }

        /// <summary>
        /// Cubic Kernel function used to determine the weight of how much each input pixel should be used to create the output pixel.
        /// b = 0, c = 0.5 (Catmull-Rom)
        /// </summary>
        /// <param name="x">Input value to the Cubic Kernel function.</param>
        /// <returns>Pixel weight.</returns>
        private static float CubicKernel(float x)
        {
            float absX = Math.Abs(x);
            float absX2 = absX * absX;
            float absX3 = absX2 * absX;

            if (absX >= 0 && absX <= 1)
            {
                return 1.5f * absX3 - 2.5f * absX2 + 1;
            }

            if (absX > 1 && absX <= 2)
            {
                return -0.5f * absX3 + 2.5f * absX2 - 4 * absX + 2;
            }

            return 0f;
        }

        #endregion
    }
}
