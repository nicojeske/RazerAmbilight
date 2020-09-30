using System;
using System.Drawing;
using Ambilight.GUI;
using Colore;
using Colore.Effects.Headset;
using Microsoft.VisualBasic;
using ColoreColor = Colore.Data.Color;

namespace Ambilight.Logic
{
    public class HeadsetLogic : IDeviceLogic
    {
        private TraySettings _settings;
        private IChroma _chroma;
        private CustomHeadsetEffect _headsetGrid = CustomHeadsetEffect.Create();

        public HeadsetLogic(TraySettings settings, IChroma chroma)
        {
            _settings = settings;
            _chroma = chroma;
        }

        public void Process(Bitmap newImage)
        {
            Bitmap mapHeadset = ImageManipulation.ResizeImage(newImage, 2, 1);
            mapHeadset = ImageManipulation.ApplySaturation(mapHeadset, _settings.Saturation);
            ApplyPictureToGrid(mapHeadset);
            _chroma.Headset.SetCustomAsync(_headsetGrid);
            mapHeadset.Dispose();
        }

        private void ApplyPictureToGrid(Bitmap map)
        {
            _headsetGrid[0] = toColoreColor(map.GetPixel(0, 0));
            _headsetGrid[1] = toColoreColor(map.GetPixel(1, 0));
        }

        private ColoreColor toColoreColor(Color color)
        {
            return new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
        }
    }
}