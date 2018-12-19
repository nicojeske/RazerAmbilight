using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Ambilight
{
    internal class Util
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
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.Bicubic;
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.PixelOffsetMode = PixelOffsetMode.None;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

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
                gr.DrawImage(srcBitmap, 0, 0, 200, 200);
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