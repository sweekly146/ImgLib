using System;
using System.Drawing;

namespace ImgLib.Position
{
    /// <summary>
    /// Bitmap positioning related functions. Name may change.
    /// </summary>
    public static class Positioner
    {
        /// <summary>
        /// Get the size that would scale the given size the most while still maintaining the same aspect ratio and fitting inside the desired size.
        /// </summary>
        /// <param name="size">Size to scale.</param>
        /// <param name="desiredSize">Desired size to scale to.</param>
        /// <returns>Maximally scaled size that fits inside the desired size.</returns>
        public static Size GetMaxScaleSameAspectRatio(Size size, Size desiredSize)
        {            
            float xRatio = desiredSize.Width / (float)size.Width;
            float yRatio = desiredSize.Height / (float)size.Height;
            
            if (xRatio < yRatio)
            {
                int scaledWidth = (int)(xRatio * size.Width);
                int scaledHeight = (int)(xRatio * size.Height);
                return new Size(scaledWidth, scaledHeight);
            }            
            else
            {
                int scaledWidth = (int)(yRatio * size.Width);
                int scaledHeight = (int)(yRatio * size.Height);
                return new Size(scaledWidth, scaledHeight);
            }
        }


        /// <summary>
        /// Centrally place a size inside another size that is logically divided into equal width subsections.
        /// </summary>
        /// <param name="size">Size that is being placed into.</param>
        /// <param name="innerSize">Size that is being placed.</param>
        /// <param name="position">Subsection position that <param name="innerSize"></param> is being placed in.</param>
        /// <param name="subsections">Amount of subsections that <param name="size"></param> is divided into.</param>
        /// <returns></returns>
        public static Point Center(Size size, Size innerSize, int position, int subsections)
        {
            if (position >= subsections || position < 0)
            {
                throw new Exception("Can't center outside of outer rectangle.");
            }
            if (subsections < 1)
            {
                throw new Exception("Must have at least one split.");
            }

            int subsectionWidth = (int)(size.Width / (float)subsections);

            if (subsectionWidth < innerSize.Width || size.Height < innerSize.Height)
            {
                throw new Exception("Inner frame size can't fit in outer frame size.");
            }

            int widthDifference = subsectionWidth - innerSize.Width;            
            float pointX = (widthDifference / 2f) + (position * subsectionWidth);            
            int roundedPointX = (int)Math.Round(pointX);            

            return new Point(roundedPointX, 0);
        }

        /// <summary>
        /// Performs a centered crop on a bitmap such that when scaled it will fit within the given size parameters. 
        /// Size will be scaled to fit the entirety of the vertical resolution within the given height.       
        /// </summary>
        /// <param name="bmp">Bitmap to crop.</param>
        /// <param name="size">Size that the cropped bitmap will fit inside of. Bitmap will be fitted vertically to this size.</param>
        /// <param name="bmps">How many bitmaps are planned to fit inside this size.</param>
        /// <param name="spaceWidth">How many pixels of spacing will be between the planned to be fitted bitmaps.</param>
        /// <returns></returns>
        public static Bitmap CenteredCrop(Bitmap bmp, Size size, int bmps, int spaceWidth)
        {
            int spaces = bmps - 1;
            int outSubWidth = (int)((size.Width - spaces * spaceWidth) / (float)bmps);
            Rectangle rectangle = GetCenteredCropRectangle(new Size(bmp.Width, bmp.Height), outSubWidth, size.Height);

            return bmp.Clone(rectangle, bmp.PixelFormat);
        }

        /// <summary>
        /// Gets a rectangle used to perform a centered crop for the given size such that when scaled it will fit within the given size parameters. 
        /// Size will be scaled to fit the entirety of the vertical resolution within the given height.       
        /// </summary>
        /// <param name="size">Size of the bitmap.</param>        
        /// <param name="width">Width that the bitmap will be fit to.</param>
        /// <param name="height">Height that the bitmap will be fit to.</param>
        /// <returns></returns>
        public static Rectangle GetCenteredCropRectangle(Size size, int width, int height)
        {            
            float ratio = size.Height / (float)height;

            //Take the minimum between the two values to make sure that the crop is kept within the bounds of the bitmap.
            int subBmpWidth = Math.Min((int)(width * ratio), size.Width);

            int centerPixel = (int)((size.Width - 1) / 2f);

            return new Rectangle(Math.Max(centerPixel - (int)(subBmpWidth / 2f), 0), 0, subBmpWidth, size.Height);
        }
    }
}