using System;
using System.Drawing;
using Ambilight.DesktopDuplication;
using Ambilight.GUI;
using Colore;
using Colore.Data;
using NLog;
using Color = Colore.Data.Color;

namespace Ambilight.Logic
{
    /// <summary>
    /// This Class manages the Logic of the software. Handling the settings, Image Manipulation and logic functions
    /// </summary>
    internal class LogicManager
    {
        private KeyboardLogic _keyboardLogic;
        private MouseLogic _mouseLogic;
        private LinkLogic _linkLogic;
        private readonly TraySettings _settings;

        public LogicManager(TraySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async void StartLogic()
        {
            //Initializing Chroma SDK
            var chromaInstance = await ColoreProvider.CreateNativeAsync();
            var appInfo = new AppInfo(
                "Ambilight for Razer devices",
                "Shows an ambilight effect on your Razer Chroma devices",
                "Nico Jeske",
                "ambilight@nicojeske.de",
                new[]
                {
                    ApiDeviceType.Keyboard,
                    ApiDeviceType.Mouse,
                    ApiDeviceType.ChromaLink
                },
                Category.Application);
            await chromaInstance.InitializeAsync(appInfo);

            _keyboardLogic = new KeyboardLogic(_settings, chromaInstance);
            _mouseLogic = new MouseLogic(_settings, chromaInstance);
            _linkLogic = new LinkLogic(_settings, chromaInstance);

            var reader = new DesktopDuplicatorReader(this, _settings);
        }

        /// <summary>
        /// Processes a captured Screenshot and create an Ambilight effect for the selected devices
        /// </summary>
        /// <param name="img"></param>
        public void ProcessNewImage(Bitmap img)
        {
            var newImage = new Bitmap(img);

            if (_settings.KeyboardEnabled)
                _keyboardLogic.Process(newImage);
            if (_settings.MouseEnabled)
                _mouseLogic.Process(newImage);
            if (_settings.LinkEnabled)
                _linkLogic.Process(newImage);

            newImage.Dispose();
        }
    }
}