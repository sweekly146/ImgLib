using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgLib.Draw
{
    /// <summary>
    /// Helper functions for the Drawer classes.
    /// </summary>
    internal class DrawHelper
    {
        /// <summary>
        /// Clamps a floating point to 0-255 and casts it to a byte.
        /// </summary>
        /// <param name="val">Specified floating point value.</param>
        /// <returns>Byte version of specified floating point.</returns>
        public static byte Clamp(int val)
        {
            return val < 0 ? (byte)0 : (byte)val;
        }
    }
}
