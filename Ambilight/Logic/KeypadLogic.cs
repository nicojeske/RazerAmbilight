using System.Drawing;
using Ambilight.GUI;
using Colore;
using Colore.Effects.Keypad;
using ColoreColor = Colore.Data.Color;

namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the mouse
    /// </summary>
    class KeypadLogic : IDeviceLogic
    {
        private readonly TraySettings _settings;
        private CustomKeypadEffect _keypadGrid = CustomKeypadEffect.Create();
        private IChroma _chroma;

        public KeypadLogic(TraySettings settings, IChroma chromaInstance)
        {
            this._settings = settings;
            this._chroma = chromaInstance;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the keypad
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        public void Process(Bitmap newImage)
        {
            Bitmap map = ImageManipulation.ResizeImage(newImage, KeypadConstants.MaxColumns, KeypadConstants.MaxRows, _settings.UltrawideModeEnabled);
            map = ImageManipulation.ApplySaturation(map, _settings.Saturation);
            ApplyPictureToGrid(map);
            _chroma.Keypad.SetCustomAsync(_keypadGrid);
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        /// <returns>EffectGrid</returns>
        private void ApplyPictureToGrid(Bitmap map)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < KeypadConstants.MaxRows ; r++)
            {
                for (var c = 0; c < KeypadConstants.MaxColumns ; c++)
                {
                    Color color;

                    if (_settings.AmbiModeEnabled)
                    {
                        color = map.GetPixel(c, KeypadConstants.MaxRows - 1);
                    }
                    else
                    {
                        color = map.GetPixel(c, r);
                    }

                    _keypadGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }
        }
    }
}
