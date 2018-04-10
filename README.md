ImgLib is a set of functions written in C# that operate on Bitmaps. Most functions are relatively well optimised as they operate on the individual bytes in the bitmap as opposed to using the GetPixel and SetPixel functions. I personally use many of these functions in other projects that I work on.

This software is not yet complete. One limitation in particular is that the scaling functions don't perform correct downsampling, so any downsampling performed, regardless of interpolation mode, will act like a nearest neighbour downsample. Most of these functions only work on bitmaps using the Format24Rgb or Format32Argb pixel formats.
