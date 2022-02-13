using AutoUpdaterDotNET;
using NLog;

namespace Ambilight
{

    /// <summary>
    /// Entry point
    /// </summary>
    internal static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Entry point. Checks for updates and initializes the software
        /// </summary>
        private static void Main()
        {
            Logger.Info("\n\n\n --- Razer Ambilight Version 3.0.S1 ----");
            AutoUpdater.Start("https://github.com/s0flY/RazerAmbilight/blob/master/ambi.xml");

            var tray = new GUI.TraySettings();
            
            Logger.Info("Tray Created");
            
            var logicManager = new Logic.LogicManager(tray);
            logicManager.StartLogic();
        }
    }
}