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
    internal class KeyboardLogic : IDeviceLogic
    {
        private readonly TraySettings _settings;
        private readonly IChroma _chroma;
        private CustomKeyboardEffect _keyboardGrid = CustomKeyboardEffect.Create();

        public KeyboardLogic(TraySettings settings, IChroma chromaInstance)
        {
            _settings = settings;
            _chroma = chromaInstance;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the keyboard
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        public void Process(Bitmap newImage)
        {
            var map = ImageManipulation.ResizeImage(newImage, _settings.KeyboardWidth, _settings.KeyboardHeight);
            map = ImageManipulation.ApplySaturation(map, _settings.Saturation);
            ApplyPictureToGrid(map);
            _chroma.Keyboard.SetCustomAsync(_keyboardGrid);
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        private void ApplyPictureToGrid(Bitmap map)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < _settings.KeyboardHeight; r++)
            {
                for (var c = 0; c < _settings.KeyboardWidth; c++)
                {
                    var color = map.GetPixel(c, r);
                    _keyboardGrid[r, c] = new ColoreColor(color.R, color.G, color.B);
                }
            }
        }
    }
}
