using System.Drawing;

namespace ImgLib.Superimpose
{
    /// <summary>
    /// Used to superimpose bitmaps onto another in parallel.
    /// </summary>
    public class BitmapPoint
    {
        public Bitmap Bitmap { get; }
        public Point Point { get; }

        /// <param name="bmp">Bitmap to be drawn onto an image.</param>
        /// <param name="point">Point to drawn the bitmap onto the image. This will be the first pixel at the top left of the destination image that will be drawn on.</param>
        public BitmapPoint(Bitmap bmp, Point point)
        {            
            Bitmap = bmp;
            Point = point;
        }
    }
}
