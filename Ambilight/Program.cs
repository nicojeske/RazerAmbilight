using AutoUpdaterDotNET;
using NLog;

namespace Ambilight
{

    /// <summary>
    /// Entry point
    /// </summary>
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Entry point. Checks for updates and initializes the software
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {

            logger.Info("\n\n\n --- Razer Ambilight Version 3.0.0 ----");
            AutoUpdater.Start("https://nicojeske.de/ambi/ambi.xml");

            GUI.TraySettings tray = new GUI.TraySettings();
            logger.Info("Tray Created");
            new Logic.LogicManager(tray);
        }
        
    }
}