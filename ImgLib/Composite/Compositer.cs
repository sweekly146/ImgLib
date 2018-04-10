using ImgLib.Scale;
using System;
using System.Drawing;

namespace ImgLib.Composite
{
    /// <summary>
    /// Image processing functions that perform more than a singular form of image processing. It is advantageous at times to combine multiple forms of image processing.
    /// Name may change.
    /// </summary>
    public static class Compositer
    {
        /// <summary>
        /// Performs a zoom operation on a bitmap and then scale the zoomed in bitmap to a specific size.
        /// The zoom and scale operations are combined so that the pixels are only interpolated once.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="zoomPercent">Percentage that the bitmap should be zoomed in by.</param>
        /// <param name="interpolationMode">Intepolation method used when scaling combined zoom/scale operation.</param>
        /// <param name="outSize">Size to scale the zoomed in bitmap to.</param>
        /// <param name="dispose">Whether or not to dispose of the source bitmap.</param>
        /// <returns>Zoomed in bitmap.</returns>
        public static Bitmap ZoomAndScale(Bitmap bmp, float zoomPercent, Scaler.InterpolationMode interpolationMode, Size outSize, bool dispose)
        {
            if (zoomPercent < 0)
            {
                throw new Exception("Can't zoom a negative percent.");
            }
            if (outSize.Width <= 0 || outSize.Height <= 0)
            {
                throw new Exception("Scaled size is non-positive. Both the width and height of the scaled size must be positive.");
            }

            Bitmap outBmp;
            Point centerPoint = new Point((int)(bmp.Width / 2f), (int)(bmp.Height / 2f));
            float zoomFactor = 1 + (zoomPercent / 100f);
            float inverseZoomFactor = 1 / zoomFactor;
            int x = centerPoint.X - (int)((bmp.Width * inverseZoomFactor) / 2f);
            int y = centerPoint.Y - (int)((bmp.Height * inverseZoomFactor) / 2f);
            int width = (int)(bmp.Width * inverseZoomFactor);
            int height = (int)(bmp.Height * inverseZoomFactor);
            Rectangle rectangle = new Rectangle(x, y, width, height);
            Bitmap croppedBmp = (Bitmap)bmp.Clone(rectangle, bmp.PixelFormat);
            outBmp = Scaler.Scale(croppedBmp, outSize, interpolationMode);

            croppedBmp.Dispose();
            if (dispose)
            {
                bmp.Dispose();
            }

            return outBmp;
        }
    }
}