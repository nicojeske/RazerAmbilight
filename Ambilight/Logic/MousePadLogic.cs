using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ambilight.GUI;
using Corale.Colore.Core;
using Corale.Colore.Razer.Mousepad.Effects;
using Color = System.Drawing.Color;
using ColoreColor = Corale.Colore.Core.Color;

namespace Ambilight.Logic
{
    /// <summary>
    /// Handles the Ambilight Effect for the mousepad
    /// </summary>
    class MousePadLogic
    {
        private TraySettings settings;

        public MousePadLogic(TraySettings settings)
        {
            this.settings = settings;
        }


        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the mousepad
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        internal void Process(Bitmap newImage)
        {
            Bitmap mapMousePad = ImageManipulation.ResizeImage(newImage, 7, 6);
            mapMousePad = ImageManipulation.ApplySaturation(mapMousePad, settings.Saturation);
            Custom mousePadGrid = Custom.Create();
            mousePadGrid = GenerateMousePadGrid(mapMousePad, mousePadGrid);

            Chroma.Instance.Mousepad.SetCustom(mousePadGrid);
            mapMousePad.Dispose();
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the mousepad
        /// </summary>
        /// <param name="mapMousePad">resized screenshot</param>
        /// <param name="mousePadGrid">effect grid</param>
        /// <returns></returns>
        private Custom GenerateMousePadGrid(Bitmap mapMousePad, Custom mousePadGrid)
        {
           
            for (int i = 0; i < 4; i++)
            {
                Color color = mapMousePad.GetPixel(6, i);
                mousePadGrid[i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }

            Color colorC = mapMousePad.GetPixel(6, 4);
            mousePadGrid[4] = new ColoreColor((byte)colorC.R, (byte)colorC.G, (byte)colorC.B);

            for (int i = 5; i >= 0; i--)
            {
                Color color = mapMousePad.GetPixel(i, 5);
                mousePadGrid[10 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }

            for (int i = 3; i >= 0; i--)
            {
                Color color = mapMousePad.GetPixel(0, i);
                mousePadGrid[14 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }


            return mousePadGrid;
     
   
            
        }
    }
}
