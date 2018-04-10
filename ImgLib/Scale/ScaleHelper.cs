namespace ImgLib.Scale
{
    /// <summary>
    /// Helper functions for the Scaler class.
    /// </summary>
    internal static class ScaleHelper
    {
        /// <summary>
        /// Clamps a floating point to 0-255 and casts it to a byte.
        /// </summary>
        /// <param name="val">Specified floating point value.</param>
        /// <returns>Byte version of specified floating point.</returns>
        internal static byte Clamp(float val)
        {
            if(val < 0)
            {
                return 0;
            }
            if(val > 255)
            {
                return 255;
            }

            return (byte)(val + 0.5f);
        }
    }
}
