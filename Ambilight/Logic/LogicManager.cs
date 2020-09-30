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
    class LogicManager
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private KeyboardLogic _keyboardLogic;
        private MousePadLogic _mousePadLogic;
        private MouseLogic _mouseLogic;
        private LinkLogic _linkLogic;
        private HeadsetLogic _headsetLogic;
        private KeypadLogic _keypadLogic;

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
            AppInfo appInfo = new AppInfo(
                "Ambilight for Razer devices",
                "Shows an ambilight effect on your Razer Chroma devices",
                "Nico Jeske",
                "ambilight@nicojeske.de",
                new[]
                {
                    ApiDeviceType.Headset,
                    ApiDeviceType.Keyboard,
                    ApiDeviceType.Keypad,
                    ApiDeviceType.Mouse,
                    ApiDeviceType.Mousepad,
                    ApiDeviceType.ChromaLink
                },
                Category.Application);
            await chromaInstance.InitializeAsync(appInfo);

            _keyboardLogic = new KeyboardLogic(settings, chromaInstance);
            _mousePadLogic = new MousePadLogic(settings, chromaInstance);
            _mouseLogic = new MouseLogic(settings, chromaInstance);
            _linkLogic = new LinkLogic(settings, chromaInstance);
            _headsetLogic = new HeadsetLogic(settings, chromaInstance);
            _keypadLogic = new KeypadLogic(settings, chromaInstance);

            DesktopDuplicatorReader reader = new DesktopDuplicatorReader(this, settings);
        }

        /// <summary>
        /// Processes a captured Screenshot and create an Ambilight effect for the selected devices
        /// </summary>
        /// <param name="newImage"></param>
        public void ProcessNewImage(Bitmap img)
        {
            Bitmap newImage = new Bitmap(img);

            if (settings.KeyboardEnabled)
                _keyboardLogic.Process(newImage);
            if (settings.PadEnabled)
                _mousePadLogic.Process(newImage);
            if (settings.MouseEnabled)
                _mouseLogic.Process(newImage);
            if (settings.LinkEnabled)
                _linkLogic.Process(newImage);
            if (settings.HeadsetEnabled)
                _headsetLogic.Process(newImage);
            if (settings.KeypadEnabeled)
                _keypadLogic.Process(newImage);

            newImage.Dispose();
        }
    }
}