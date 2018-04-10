using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImgLib.Superimpose
{
    /// <summary>
    /// Functions for superimposing one bitmap onto another in parallel.
    /// TODO: Make copy work in parallel.
    /// </summary>
    public class ParallelSuperimposer
    {
        /// <summary>
        /// Blends a list of bitmap points with a background bitmap. Bitmaps are blended in parallel. Visual anomalies will appear if bitmap points overlap.
        /// </summary>
        /// <param name="backBmp">Bitmap to be drawn onto.</param>        
        /// <param name="bitmapPoints">Collection of BitmapPoints to be drawn onto the back bitmap.</param>
        public static void BlendBitmapPoints(Bitmap backBmp, IEnumerable<BitmapPoint> bitmapPoints)
        {
            unsafe
            {                
                BitmapData backBmpData = backBmp.LockBits(new Rectangle(0, 0, backBmp.Width, backBmp.Height), ImageLockMode.ReadWrite, backBmp.PixelFormat);
                int backBmpWidth = backBmp.Width;
                int backBmpHeight = backBmp.Height;

                Parallel.ForEach(bitmapPoints, bitmapPoint =>
                {
                    if (bitmapPoint?.Bitmap == null)
                    {
                        return;
                    }

                    Point point = bitmapPoint.Point;                    
                    Bitmap frontBmp = bitmapPoint.Bitmap;               
                    Superimposer.Superimpose(backBmpData, frontBmp, true, point);
                });

                backBmp.UnlockBits(backBmpData);
            }
        }

        /// <summary>
        /// Blends a background bitmap with bitmap points from a list of buffers. Bitmaps are blended in parallel. Visual anomalies will appear if bitmap points overlap.
        /// </summary>
        /// <param name="backBmp">Bitmap to be drawn onto.</param>        
        /// <param name="bitmapPointBuffers">List of buffers containing BitmapPoints.</param>
        public static void BlendBitmapPointBuffers(Bitmap backBmp, List<BlockingCollection<BitmapPoint>> bitmapPointBuffers)
        {
            unsafe
            {
                BitmapData backBmpData = backBmp.LockBits(new Rectangle(0, 0, backBmp.Width, backBmp.Height), ImageLockMode.ReadWrite, backBmp.PixelFormat);
                int backBmpWidth = backBmp.Width;
                int backBmpHeight = backBmp.Height;

                Parallel.ForEach(bitmapPointBuffers, bitmapPointBuffer =>
                {
                    if (bitmapPointBuffer.IsCompleted == false)
                    {
                        BitmapPoint bitmapPoint = bitmapPointBuffer.Take();

                        if (bitmapPoint?.Bitmap == null)
                        {
                            return;
                        }

                        Point point = bitmapPoint.Point;
                        Bitmap frontBmp = bitmapPoint.Bitmap;
                        Superimposer.Superimpose(backBmpData, frontBmp, true, point);

                    }
                });            
            
                backBmp.UnlockBits(backBmpData);
            }
        }

        /// <summary>
        /// Blends bitmap points that come from a list of buffered lists of BitmapPoints with a background bitmap. Bitmaps are blended in parallel. Visual anomalies will appear if bitmap points overlap.
        /// </summary>
        /// <param name="backBmp">Bitmap to be drawn onto.</param>        
        /// <param name="bitmapPointBuffers">List of buffered lists of BitmapPoints.</param>
        public static void BlendBitmapPointsBuffers(Bitmap backBmp, List<BlockingCollection<List<BitmapPoint>>> bitmapPointsBuffers)
        {
            unsafe
            {
                BitmapData backBmpData = backBmp.LockBits(new Rectangle(0, 0, backBmp.Width, backBmp.Height), ImageLockMode.ReadWrite, backBmp.PixelFormat);
                int backBmpWidth = backBmp.Width;
                int backBmpHeight = backBmp.Height;

                List<BitmapPoint> bitmapPoints = new List<BitmapPoint>();
                for (int i = 0; i < bitmapPointsBuffers.Count; i++)
                {
                    if (bitmapPointsBuffers[i].IsCompleted == false)
                    {
                        List<BitmapPoint> currentBitmapPoints = bitmapPointsBuffers[i].Take();
                        foreach (BitmapPoint bitmapPoint in currentBitmapPoints)
                        {
                            if (bitmapPoint?.Bitmap != null)
                            {
                                bitmapPoints.Add(bitmapPoint);
                            }
                        }
                    }
                }

                Parallel.ForEach(bitmapPoints, bitmapPoint =>
                {
                    if (bitmapPoint?.Bitmap == null)
                    {
                        return;
                    }

                    Point point = bitmapPoint.Point;
                    Bitmap frontBmp = bitmapPoint.Bitmap;
                    Superimposer.Superimpose(backBmpData, frontBmp, true, point);
                });

                backBmp.UnlockBits(backBmpData);
            }
        }

    }
}
