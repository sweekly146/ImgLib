using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImgLib.Draw
{
    /// <summary>
    /// Functions for drawing text to a bitmap.
    /// TODO: Remove some graphics configuration?
    /// </summary>
    public static class TextDrawer
    {
        /// <summary>
        /// Draw a text counter to a bitmap using a border to improve readability on variable backgrounds.        
        /// </summary>
        /// <param name="borderColor">Color of the border surrounding the text.</param>
        /// <param name="prefixColor">Color of the prefix text.</param>
        /// <param name="valueColor">Color of the counter text.</param>
        /// <param name="font">Font used to draw the text.</param>
        /// <param name="borderWidth">Width of the border.</param>
        /// <param name="prefix">Prefix text.</param>
        /// <param name="value">Counter value.</param>
        /// <returns>Bitmap containing the text counter.</returns>
        public static Bitmap DrawCounter(Color borderColor, Color prefixColor, Color valueColor, Font font, float borderWidth, string prefix, string value)
        {
            Bitmap dummyBmp = new Bitmap(1, 1);
            SizeF labelLength, valueLength;
            using (Graphics graphics = Graphics.FromImage(dummyBmp))
            {
                labelLength = graphics.MeasureString(prefix, font);
                valueLength = graphics.MeasureString(value, font);
            }
            dummyBmp.Dispose();

            Size bmpSize = new Size((int)Math.Ceiling(labelLength.Width + valueLength.Width), (int)Math.Max(labelLength.Height, valueLength.Height));
            Bitmap bmp = new Bitmap(bmpSize.Width, bmpSize.Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                GraphicsPath valuePath = new GraphicsPath();
                GraphicsPath prefixPath = new GraphicsPath();

                RectangleF labelRectangle = new RectangleF(0, 0, bmp.Width, bmp.Height);

                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;                
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                valuePath.AddString(prefix + value, font.FontFamily, (int)font.Style, font.Size, labelRectangle, StringFormat.GenericDefault);
                graphics.FillPath(new SolidBrush(valueColor), valuePath);
                graphics.DrawPath(new Pen(new SolidBrush(borderColor), borderWidth), valuePath);

                prefixPath.AddString(prefix, font.FontFamily, (int)font.Style, font.Size, labelRectangle, StringFormat.GenericDefault);
                graphics.FillPath(new SolidBrush(prefixColor), prefixPath);
                graphics.DrawPath(new Pen(new SolidBrush(borderColor), borderWidth), prefixPath);
            }

            return bmp;
        }

        /// <summary>
        /// Draw a text counter to a bitmap using a border to improve readability on variable backgrounds.        
        /// </summary>
        /// <param name="borderColor">Color of the border surrounding the text.</param>
        /// <param name="prefixColor">Color of the prefix text.</param>
        /// <param name="suffixColor">Color of the suffix text.</param>
        /// <param name="valueColor">Color of the counter text.</param>
        /// <param name="font">Font used to draw the text.</param>
        /// <param name="borderWidth">Width of the border.</param>
        /// <param name="prefix">Prefix text.</param>
        /// <param name="suffix">Suffix text.</param>
        /// <param name="value">Counter value.</param>
        /// <returns></returns>
        public static Bitmap DrawCounter(Color borderColor, Color prefixColor, Color suffixColor, Color valueColor, Font font, float borderWidth, string prefix, string suffix, string value)
        {
            Bitmap dummyBmp = new Bitmap(1, 1);
            SizeF labelLength, valueLength, suffixLength;
            using (Graphics graphics = Graphics.FromImage(dummyBmp))
            {
                labelLength = graphics.MeasureString(prefix, font);
                valueLength = graphics.MeasureString(value, font);
                suffixLength = graphics.MeasureString(suffix, font);
            }
            dummyBmp.Dispose();

            Size bmpSize = new Size((int)Math.Ceiling(labelLength.Width + valueLength.Width + suffixLength.Width), (int)Math.Max(suffixLength.Height, Math.Max(labelLength.Height, valueLength.Height)));
            Bitmap bmp = new Bitmap(bmpSize.Width, bmpSize.Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                GraphicsPath valuePath = new GraphicsPath();
                GraphicsPath prefixPath = new GraphicsPath();
                GraphicsPath suffixPath = new GraphicsPath();

                RectangleF labelRectangle = new RectangleF(0, 0, bmp.Width, bmp.Height);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                suffixPath.AddString(prefix + value + suffix, font.FontFamily, (int)font.Style, font.Size, labelRectangle, StringFormat.GenericDefault);
                graphics.FillPath(new SolidBrush(suffixColor), suffixPath);
                graphics.DrawPath(new Pen(new SolidBrush(borderColor), borderWidth), suffixPath);

                valuePath.AddString(prefix + value, font.FontFamily, (int)font.Style, font.Size, labelRectangle, StringFormat.GenericDefault);
                graphics.FillPath(new SolidBrush(valueColor), valuePath);
                graphics.DrawPath(new Pen(new SolidBrush(borderColor), borderWidth), valuePath);

                prefixPath.AddString(prefix, font.FontFamily, (int)font.Style, font.Size, labelRectangle, StringFormat.GenericDefault);
                graphics.FillPath(new SolidBrush(prefixColor), prefixPath);
                graphics.DrawPath(new Pen(new SolidBrush(borderColor), borderWidth), prefixPath);
            }

            return bmp;
        }
    }
}
