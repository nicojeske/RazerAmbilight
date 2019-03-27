using Ambilight.DesktopDuplication;
using Corale.Colore.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly GUI.TraySettings settings;

        public LogicManager(GUI.TraySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            //Initializing Chroma SDK
            Chroma.Instance.Initialize();
            _keyboardLogic = new KeyboardLogic(settings);
            _mousePadLogic = new MousePadLogic(settings);
            _mouseLogic = new MouseLogic(settings);

            DesktopDuplicatorReader reader = new DesktopDuplicatorReader(this, settings);


            while (true)
            {
                Thread.Sleep(1000 / settings.Tickrate);
            }
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

         
            


            newImage.Dispose();
        }
    }
}
