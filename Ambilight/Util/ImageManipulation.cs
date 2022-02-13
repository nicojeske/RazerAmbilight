using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Ambilight.Util;

namespace Ambilight
{
    internal static class ImageManipulation
    {
        /// <summary>
        /// Resize an image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {           
            return new Bitmap(image, width, height);
        }

        /// <summary>
        /// Applies a given saturation value to a Bitmap.
        /// </summary>
        /// <param name="srcBitmap">Bitmap</param>
        /// <param name="saturation">Saturation Value</param>
        /// <returns></returns>
        public static Bitmap ApplySaturation(Bitmap srcBitmap, float saturation)
        {
            const float rWeight = 0.3086f;
            const float gWeight = 0.6094f;
            const float bWeight = 0.0820f;

            var a = (1.0f - saturation) * rWeight + saturation;
            var b = (1.0f - saturation) * rWeight;
            var c = (1.0f - saturation) * rWeight;
            var d = (1.0f - saturation) * gWeight;
            var e = (1.0f - saturation) * gWeight + saturation;
            var f = (1.0f - saturation) * gWeight;
            var g = (1.0f - saturation) * bWeight;
            var h = (1.0f - saturation) * bWeight;
            var i = (1.0f - saturation) * bWeight + saturation;

            var returnBitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height);

            // Create a Graphics
            using (var gr = Graphics.FromImage(returnBitmap))
            {
                // ColorMatrix elements
                float[][] ptsArray = {
                    new[] {a,  b,  c,  0, 0},
                    new[] {d,  e,  f,  0, 0},
                    new[] {g,  h,  i,  0, 0},
                    new float[] {0,  0,  0,  1, 0},
                    new float[] {0, 0, 0, 0, 1}
                };
                // Create ColorMatrix
                var clrMatrix = new ColorMatrix(ptsArray);
                // Create ImageAttributes
                var imageAttributes = new ImageAttributes();
                // Set color matrix
                imageAttributes.SetColorMatrix(clrMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Default);
                // Draw Image with image attributes
                gr.DrawImage(srcBitmap,
                    new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height),
                    0, 0, srcBitmap.Width, srcBitmap.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }
            return returnBitmap;
        }
    }
}