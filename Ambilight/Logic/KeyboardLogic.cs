using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corale;
using Corale.Colore.Core;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;
using ColoreColor = Corale.Colore.Core.Color;


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
            Bitmap map = ImageManipulation.ResizeImage(newImage, settings.KeyboardWidth, settings.KeyboardHeight);
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
                    System.Drawing.Color color = map.GetPixel(c, r);

                    keyboardGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }

            return keyboardGrid;
        }
    }
}
