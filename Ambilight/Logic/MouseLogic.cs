using Corale.Colore.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corale.Colore.Razer.Mouse.Effects;
using ColoreColor = Corale.Colore.Core.Color;
using Ambilight.GUI;

namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the mouse
    /// </summary>
    class MouseLogic
    {
        private TraySettings settings;

        public MouseLogic(TraySettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the mouse
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        internal void Process(Bitmap newImage)
        {
            var mouseGrid = Corale.Colore.Razer.Mouse.Effects.CustomGrid.Create();
            Bitmap mapMouse = ImageManipulation.ResizeImage(newImage, Corale.Colore.Razer.Mouse.Constants.MaxColumns,
                    Corale.Colore.Razer.Mouse.Constants.MaxRows);

            

            mouseGrid = GenerateMouseGrid(mapMouse, mouseGrid);
            Chroma.Instance.Mouse.SetGrid(mouseGrid);
            mapMouse.Dispose();
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the mouse
        /// </summary>
        /// <param name="mapMousePad">resized screenshot</param>
        /// <param name="mousePadGrid">effect grid</param>
        /// <returns>EffectGrid</returns>
        private CustomGrid GenerateMouseGrid(Bitmap mapMouse, CustomGrid mouseGrid)
        {

            for (var r = 0; r < Corale.Colore.Razer.Mouse.Constants.MaxRows; r++)
            {
                for (var c = 0; c < Corale.Colore.Razer.Mouse.Constants.MaxColumns; c++)
                {
                    System.Drawing.Color color;

                    if (settings.AmbiModeBool)
                        color = mapMouse.GetPixel(6, 8);
                    else
                        color = mapMouse.GetPixel(c, r);


                    mouseGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }

            return mouseGrid;
        }
    }


}
