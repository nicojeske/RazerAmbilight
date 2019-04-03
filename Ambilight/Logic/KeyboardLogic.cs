using Corale.Colore.Core;
using System.Drawing;
using ColoreColor = Corale.Colore.Core.Color;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;


namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the mouse
    /// </summary>
    class KeyboardLogic
    {
        private GUI.TraySettings settings;
        private KeyboardCustom _keyboardGrid;

        public KeyboardLogic(GUI.TraySettings settings)
        {
            this.settings = settings;            
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the keyboard
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        internal void Process(Bitmap newImage)
        {
            Bitmap map = ImageManipulation.ResizeImage(newImage, settings.KeyboardWidth, settings.KeyboardHeight, settings.UltrawideModeBool);
            map = ImageManipulation.ApplySaturation(map, settings.Saturation);
            _keyboardGrid = KeyboardCustom.Create();
            _keyboardGrid = GenerateKeyboardGrid(map, _keyboardGrid);
            Chroma.Instance.Keyboard.SetCustom(_keyboardGrid);            
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        /// <param name="keyboardGrid">effect grid</param>
        /// <returns>EffectGrid</returns>
        private KeyboardCustom GenerateKeyboardGrid(Bitmap map, KeyboardCustom keyboardGrid)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < settings.KeyboardHeight; r++)
            {
                for (var c = 0; c < settings.KeyboardWidth; c++)
                {
                    System.Drawing.Color color;

                    if (settings.AmbiModeBool)
                        color = map.GetPixel(c, settings.KeyboardHeight - 1);
                    else
                        color = map.GetPixel(c, r);

                    keyboardGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }            

            return keyboardGrid;
        }
    }
}
