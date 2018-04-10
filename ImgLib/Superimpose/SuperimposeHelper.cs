using System;
using System.Drawing;

namespace ImgLib.Superimpose
{
    /// <summary>
    /// Helper functions for the Superimposer and ParallelSuperimposer class.
    /// </summary>
    internal class SuperimposerHelper
    {
        /// <summary>
        /// Clamps a floating point to 0-255 and casts it to a byte.
        /// </summary>
        /// <param name="val">Specified floating point value.</param>
        /// <returns>Byte version of specified floating point.</returns>
        internal static byte Clamp(float val)
        {
            if (val < 0)
            {
                return 0;
            }
            if (val > 255)
            {
                return 255;
            }

            return (byte)(val + 0.5f);
        }

        /// <summary>
        /// Gets the size of the area from the front bitmap that should be used for a superimpose operation.
        /// </summary>                
        /// <param name="frontBmp">Bitmap to draw from.</param>        
        /// <param name="point">Point on the back bitmap to draw the front bitmap.</param>     
        /// <param name="backBmpWidth">Width bitmap to be drawn onto.</param>
        /// <param name="backBmpHeight">Height bitmap to be drawn onto.</param> 
        /// <returns>Size of the area from the front bitmap that should be used for a superimpose operation</returns>
        internal static Size GetFrontBmpSize(Bitmap frontBmp, Point point, int backBmpWidth, int backBmpHeight)
        {
            int width = frontBmp.Width;
            int height = frontBmp.Height;
            int backBmpWidthDistance = backBmpWidth - point.X;
            int backBmpHeightDistance = backBmpHeight - point.Y;
            width = Math.Min(width, backBmpWidthDistance);
            height = Math.Min(height, backBmpHeightDistance);
            Size size = new Size(width, height);

            return size;
        }
    }
}
