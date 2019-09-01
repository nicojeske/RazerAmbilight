using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambilight.Util
{
    public static class Extensions
    {
        public static Bitmap CropAtRectangle(this Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                nb.SetResolution(r.Width, r.Height);
                return nb;
            }                    
        }
    }
}
