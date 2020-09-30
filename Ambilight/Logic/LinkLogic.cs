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
            Bitmap map = ImageManipulation.ResizeImage(newImage, 4, 1);
            map = ImageManipulation.ApplySaturation(map, _settings.Saturation);
            
            ApplyImageToGrid(map);
            ApplyC1(ImageManipulation.ResizeImage(map, 1, 1));
            _chroma.ChromaLink.SetCustomAsync(_linkGrid);
            map.Dispose();
        }

        private void ApplyC1(Bitmap map)
        {
            Color color = map.GetPixel(0,0);
            _linkGrid[0] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
        }

        /// <summary>
        /// From a given resized screenshot, an ambilight effect will be created for the keyboard
        /// </summary>
        /// <param name="map">resized screenshot</param>
        private void ApplyImageToGrid(Bitmap map)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (int i = 1; i < Colore.Effects.ChromaLink.ChromaLinkConstants.MaxLeds; i++)
            {
                Color color = map.GetPixel(i-1,0);
                _linkGrid[i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }
        }
    }
}
