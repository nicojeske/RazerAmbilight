using System.Drawing;
using Ambilight.GUI;
using Colore;
using Colore.Effects.ChromaLink;
using ColoreColor = Colore.Data.Color;


namespace Ambilight.Logic
{

    /// <summary>
    /// Handles the Ambilight Effect for the Link connection
    /// </summary>
    class LinkLogic : IDeviceLogic
    {
        private GUI.TraySettings _settings;
        private CustomChromaLinkEffect _linkGrid = CustomChromaLinkEffect.Create();
        private IChroma _chroma;
        
        public LinkLogic(TraySettings settings, IChroma chromaInstance)
        {
            this._settings = settings;
            this._chroma = chromaInstance;
        }

        /// <summary>
        /// Processes a ScreenShot and creates an Ambilight Effect for the chroma link
        /// </summary>
        /// <param name="newImage">ScreenShot</param>
        public void Process(Bitmap newImage)
        {
            Bitmap map = ImageManipulation.ResizeImage(newImage, 2, 2);
            map = ImageManipulation.ApplySaturation(map, _settings.Saturation);
            GenerateLinkGrid(map);

            _chroma.ChromaLink.SetCustomAsync(_linkGrid);
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        private void GenerateLinkGrid(Bitmap map)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 1; r <= 2; r++)
            {
                for (var c = 0; c < 2; c++)
                {
                    System.Drawing.Color color;
                    color = map.GetPixel(c, r-1);
                    if (r == 2) r++;
                    _linkGrid[c + r] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                    if (r > 2) r--;
                }
            }
            _linkGrid[0] = _linkGrid[1];
        }
    }
}
