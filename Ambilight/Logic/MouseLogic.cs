using System.Drawing;
using Ambilight.GUI;
using Colore;
using Colore.Effects.Mouse;
using ColoreColor = Colore.Data.Color;

namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the mouse
    /// </summary>
    class MouseLogic : IDeviceLogic
    {
        private TraySettings _settings;
        private IChroma _chroma;
        private CustomMouseEffect _mouseGrid = CustomMouseEffect.Create();

        public MouseLogic(TraySettings settings, IChroma chromaInstance)
        {
            this._settings = settings;
            this._chroma = chromaInstance;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the mouse
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        public void Process(Bitmap newImage)
        {
            Bitmap mapMouse = ImageManipulation.ResizeImage(newImage, MouseConstants.MaxColumns,
                    MouseConstants.MaxRows);
            mapMouse = ImageManipulation.ApplySaturation(mapMouse, _settings.Saturation);            
            ApplyPictureToGrid(mapMouse);
            _chroma.Mouse.SetGridAsync(_mouseGrid);
            mapMouse.Dispose();
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the mouse
        /// </summary>
        /// <param name="mapMousePad">resized screenshot</param>
        /// <param name="mousePadGrid">effect grid</param>
        /// <returns>EffectGrid</returns>
        private void ApplyPictureToGrid(Bitmap mapMouse)
        {

            for (var r = 0; r < MouseConstants.MaxRows; r++)
            {
                for (var c = 0; c < MouseConstants.MaxColumns; c++)
                {
                    Color color;

                    if (_settings.AmbiModeEnabled)
                        color = mapMouse.GetPixel(6, 8);
                    else
                        color = mapMouse.GetPixel(c, r);


                    _mouseGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }
        }
    }


}
