using ImgLib.Scale;
using System;
using System.Drawing;

namespace ImgLib.Zoom
{
    /// <summary>
    /// Zoom related functions for bitmaps.
    /// </summary>
    public static class Zoomer
    {
        /// <summary>
        /// Gets the zoomed version of a given bitmap.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <param name="zoomPercent">Percentage that the bitmap should be zoomed in by.</param>
        /// <param name="interpolationMode">Intepolation method used when scaling for the zoom operation.</param>
        /// <param name="dispose">Whether or not to dispose of the source bitmap.</param>
        /// <returns>Zoomed in bitmap.</returns>
        public static Bitmap Zoom(Bitmap bmp, float zoomPercent, Scaler.InterpolationMode interpolationMode, bool dispose)
        {
            if (zoomPercent < 0)
            {
                throw new Exception("Can't zoom a negative percent.");
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
            outBmp = Scaler.Scale(croppedBmp, bmp.Size, interpolationMode);

            croppedBmp.Dispose();

            if (dispose)
            {
                bmp.Dispose();
            }

            return outBmp;
        }
    }
}