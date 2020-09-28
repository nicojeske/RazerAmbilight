using Corale.Colore.Core;
using System.Drawing;
using ColoreColor = Corale.Colore.Core.Color;
using LinkCustom = Corale.Colore.Razer.ChromaLink.Effects.Custom;


namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the Link connection
    /// </summary>
    class LinkLogic
    {
        private GUI.TraySettings settings;
        private LinkCustom _linkGrid;

        public LinkLogic(GUI.TraySettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the chroma link
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        internal void Process(Bitmap newImage)
        {
            Bitmap map = ImageManipulation.ResizeImage(newImage, 2, 2);
            map = ImageManipulation.ApplySaturation(map, settings.Saturation);
            _linkGrid = LinkCustom.Create();
            _linkGrid = GenerateLinkGrid(map, _linkGrid);
            Chroma.Instance.ChromaLink.SetCustom(_linkGrid);
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        /// <param name="LinkGrid">effect grid</param>
        /// <returns>EffectGrid</returns>
        private LinkCustom GenerateLinkGrid(Bitmap map, LinkCustom linkGrid)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 1; r <= 2; r++)
            {
                for (var c = 0; c < 2; c++)
                {
                    System.Drawing.Color color;
                    color = map.GetPixel(c, r-1);
                    if (r == 2) r++;
                    linkGrid[c + r] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                    if (r > 2) r--;
                }
            }
            linkGrid[0] = linkGrid[1];
            return linkGrid;
        }
    }
}
