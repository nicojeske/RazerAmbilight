using System.Drawing;

namespace Ambilight.Util
{
    public static class Extensions
    {
        public static Bitmap CropAtRectangle(this Bitmap bitmap, Rectangle rectangle)
        {
            Bitmap newBitmap = new Bitmap(rectangle.Width, rectangle.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(bitmap, -rectangle.X, -rectangle.Y);
                bitmap.Dispose();
                newBitmap.SetResolution(rectangle.Width, rectangle.Height);
                return newBitmap;
            }
        }
    }
}