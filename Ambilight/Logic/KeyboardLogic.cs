using System.Drawing;
using Ambilight.GUI;
using Colore;
using Colore.Effects.Keyboard;
using ColoreColor = Colore.Data.Color;

namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the mouse
    /// </summary>
    class KeyboardLogic : IDeviceLogic
    {
        private readonly GUI.TraySettings _settings;
        private CustomKeyboardEffect _keyboardGrid = Colore.Effects.Keyboard.CustomKeyboardEffect.Create();
        private IChroma _chroma;

        public KeyboardLogic(TraySettings settings, IChroma chromaInstance)
        {
            this._settings = settings;
            this._chroma = chromaInstance;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the keyboard
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        public void Process(Bitmap newImage)
        {
            Bitmap map = ImageManipulation.ResizeImage(newImage, _settings.KeyboardWidth, _settings.KeyboardHeight, _settings.UltrawideModeBool);
            map = ImageManipulation.ApplySaturation(map, _settings.Saturation);
            ApplyPictureToGrid(map);
            _chroma.Keyboard.SetCustomAsync(_keyboardGrid);
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        /// <returns>EffectGrid</returns>
        private void ApplyPictureToGrid(Bitmap map)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < _settings.KeyboardHeight; r++)
            {
                for (var c = 0; c < _settings.KeyboardWidth; c++)
                {
                    System.Drawing.Color color;

                    if (_settings.AmbiModeBool)
                    {
                        color = map.GetPixel(c, _settings.KeyboardHeight - 1);
                    }
                    else
                    {
                        color = map.GetPixel(c, r);
                    }

                    _keyboardGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }
        }
    }
}
