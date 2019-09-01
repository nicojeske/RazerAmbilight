using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Ambilight.Util;
using NLog;

namespace Ambilight
{
    class ImageManipulation
    {

        private static Logger _log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Resize an image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, bool cropSides = false)
        {           
            try
            {
                if (cropSides)
                {
                    // Cuts down a 21:9 image to a 16:9 image by removing the outer sides
                    using (var croppedImage = new Bitmap(image).CropAtRectangle(new Rectangle(Convert.ToInt32((image.Width / 21) * 2.5), 0, (image.Width / 21) * 16, image.Height)))
                    {
                        return new Bitmap(croppedImage, width, height);
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo: Log this exception. Just catching in case there are memory issues with the Bitmap. Shouldn't happen though.
                _log.Error(ex, $"Error while resizing the image. width: {width} height: {height} cropSides: {cropSides}");
            }            

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
            float rWeight = 0.3086f;
            float gWeight = 0.6094f;
            float bWeight = 0.0820f;

            float a = (1.0f - saturation) * rWeight + saturation;
            float b = (1.0f - saturation) * rWeight;
            float c = (1.0f - saturation) * rWeight;
            float d = (1.0f - saturation) * gWeight;
            float e = (1.0f - saturation) * gWeight + saturation;
            float f = (1.0f - saturation) * gWeight;
            float g = (1.0f - saturation) * bWeight;
            float h = (1.0f - saturation) * bWeight;
            float i = (1.0f - saturation) * bWeight + saturation;

            Bitmap returnBitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height);

            // Create a Graphics
            using (Graphics gr = Graphics.FromImage(returnBitmap))
            {
                // ColorMatrix elements
                float[][] ptsArray = {
                    new float[] {a,  b,  c,  0, 0},
                    new float[] {d,  e,  f,  0, 0},
                    new float[] {g,  h,  i,  0, 0},
                    new float[] {0,  0,  0,  1, 0},
                    new float[] {0, 0, 0, 0, 1}
                };
                // Create ColorMatrix
                ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
                // Create ImageAttributes
                ImageAttributes imgAttribs = new ImageAttributes();
                // Set color matrix
                imgAttribs.SetColorMatrix(clrMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Default);
                // Draw Image with no effects
                //gr.DrawImage(srcBitmap, 0, 0, 200, 200);
                // Draw Image with image attributes
                gr.DrawImage(srcBitmap,
                    new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height),
                    0, 0, srcBitmap.Width, srcBitmap.Height,
                    GraphicsUnit.Pixel, imgAttribs);
            }

            return returnBitmap;
        }
    }
}