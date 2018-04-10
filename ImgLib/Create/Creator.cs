using ImgLib.Draw;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImgLib.Create
{
    /// <summary>
    /// Bitmap creation functions.
    /// </summary>
    public static class Creator
    {
        /// <summary>
        /// Creates a bitmap with every pixel set to a specified color.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="color">Specified color.</param>
        /// <param name="format">Bitmap pixel format.</param>
        /// <returns>Colored bitmap.</returns>
        public static Bitmap CreateColoredBitmap(int width, int height, Color color, PixelFormat format)
        {
            if (height <= 0 || width <= 0)
            {
                throw new Exception("Height or Width parameter is non-positive. Height and Width must be positive.");
            }
            //Currently only supports Format24bppRgb and Format32bppArgb.
            if (format != PixelFormat.Format24bppRgb && format != PixelFormat.Format32bppArgb)
            {
                throw new Exception("Invalid Pixel Format. Pixel Format must be either Format24bppRgb or Format32bppArgb");
            }

            Bitmap bmp = new Bitmap(width, height, format);
            Drawer.FillRectangleWithColor(bmp, color, new Rectangle(0, 0, width, height));

            return bmp;
        }

        /// <summary>
        /// Creates a bitmap with every pixel set to a specified color.
        /// </summary>        
        /// <param name="size">Bitmap size.</param>
        /// <param name="color">Specified color.</param>
        /// <param name="format">Bitmap pixel format.</param>
        /// <returns>Colored bitmap.</returns>
        public static Bitmap CreateColoredBitmap(Size size, Color color, PixelFormat format)
        {
            return CreateColoredBitmap(size.Width, size.Height, color, format);
        }
    }
}