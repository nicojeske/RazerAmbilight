using System.Drawing;
using Ambilight.GUI;
using Colore;
using Colore.Effects.Mousepad;
using ColoreColor = Colore.Data.Color;

namespace Ambilight.Logic
{
    /// <summary>
    /// Handles the Ambilight Effect for the mousepad
    /// </summary>
    class MousePadLogic : IDeviceLogic
    {
        private TraySettings _settings;
        private IChroma _chroma;
        private CustomMousepadEffect _mousepadGrid = CustomMousepadEffect.Create();

        public MousePadLogic(TraySettings settings, IChroma chromaInstance)
        {
            this._settings = settings;
            this._chroma = chromaInstance;
        }


        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the mousepad
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        public void Process(Bitmap newImage)
        {
            Bitmap mapMousePad = ImageManipulation.ResizeImage(newImage, 7, 6);
            mapMousePad = ImageManipulation.ApplySaturation(mapMousePad, _settings.Saturation);
            ApplyPictureToGrid(mapMousePad);

            _chroma.Mousepad.SetCustomAsync(_mousepadGrid);
            mapMousePad.Dispose();
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the mousepad
        ///^->>i
        ///^   V
        ///^   V
        ///<<<<V
        ///  
        /// </summary>
        /// <param name="mapMousePad">resized screenshot</param>
        /// <returns></returns>
        private void ApplyPictureToGrid(Bitmap mapMousePad)
        {

            if (!_settings.AmbiModeEnabled)
            {
                for (int i = 0; i < 4; i++)
                {
                    Color color = mapMousePad.GetPixel(6, i);
                    _mousepadGrid[i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }

                Color colorC = mapMousePad.GetPixel(6, 4);
                _mousepadGrid[4] = new ColoreColor((byte)colorC.R, (byte)colorC.G, (byte)colorC.B);

                for (int i = 5; i >= 0; i--)
                {
                    Color color = mapMousePad.GetPixel(i, 5);
                    _mousepadGrid[10 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }

                for (int i = 3; i >= 0; i--)
                {
                    Color color = mapMousePad.GetPixel(0, i);
                    _mousepadGrid[14 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            } else
            {
                //RIGHT
                for (int i = 0; i < 4; i++)
                {
                    Color color = mapMousePad.GetPixel(6, 5);
                    _mousepadGrid[i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }

                //RIGHT DOWN CORNOR
                Color colorC = mapMousePad.GetPixel(6, 5);
                _mousepadGrid[4] = new ColoreColor((byte)colorC.R, (byte)colorC.G, (byte)colorC.B);

                //BOTTOM
                for (int i = 5; i >= 0; i--)
                {
                    Color color = mapMousePad.GetPixel(5, 5);
                    _mousepadGrid[10 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }

                ///CORNER + LEFT
                for (int i = 3; i >= 0; i--)
                {
                    Color color = mapMousePad.GetPixel(4, 5);
                    _mousepadGrid[14 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }
        }
    }
}
