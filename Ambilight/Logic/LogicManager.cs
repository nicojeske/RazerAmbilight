using System;
using System.Drawing;
using Ambilight.DesktopDuplication;
using Ambilight.GUI;
using Colore;
using NLog;
using Color = Colore.Data.Color;

namespace Ambilight.Logic
{
    /// <summary>
    /// This Class manages the Logic of the software. Handling the settings, Image Manipulation and logic functions
    /// </summary>
    class LogicManager
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private KeyboardLogic _keyboardLogic;
        private MousePadLogic _mousePadLogic;
        private MouseLogic _mouseLogic;
        private LinkLogic _linkLogic;

        private readonly TraySettings settings;

        public LogicManager(TraySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            this.StartLogic(settings);
        }

        private async void StartLogic(TraySettings settings)
        {
            //Initializing Chroma SDK
            IChroma chromaInstance = await ColoreProvider.CreateNativeAsync();

            _keyboardLogic = new KeyboardLogic(settings, chromaInstance);
            _mousePadLogic = new MousePadLogic(settings, chromaInstance);
            _mouseLogic = new MouseLogic(settings, chromaInstance);
            _linkLogic = new LinkLogic(settings, chromaInstance);

            DesktopDuplicatorReader reader = new DesktopDuplicatorReader(this, settings);
        }

        /// <summary>
        /// Processes a captured Screenshot and create an Ambilight effect for the selected devices
        /// </summary>
        /// <param name="newImage"></param>
        public void ProcessNewImage(Bitmap test)
        {
            //newImage = ImageManipulation.ApplySaturation(newImage, settings.Saturation);
            Bitmap newImage = new Bitmap(test);

            if (settings.KeyboardEnabledBool)
                _keyboardLogic.Process(newImage);
            if (settings.PadEnabledBool)
                _mousePadLogic.Process(newImage);
            if (settings.MouseEnabledBool)
                _mouseLogic.Process(newImage);
            if (settings.LinkEnabledBool)
                _linkLogic.Process(newImage);

            newImage.Dispose();
        }
    }
}