using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Ambilight.Properties;
using Colore.Effects.Keyboard;
using IWshRuntimeLibrary;
using Microsoft.VisualBasic;
using NLog;
using File = System.IO.File;

namespace Ambilight.GUI
{
    /// <summary>
    /// This class handles the settings, as well as the tray icon
    /// </summary>
    public class TraySettings
    {
        public float Saturation { get; private set; }
        public int KeyboardWidth { get; private set; }
        public int KeyboardHeight { get; private set; }
        public bool KeyboardEnabled { get; private set; }
        public bool MouseEnabled { get; private set; }
        public bool LinkEnabled { get; private set; }
        public int SelectedMonitor { get; private set; }
        private int Tickrate { get; set; }
        private bool AutostartEnabled { get; set; }
        
        private NotifyIcon _notifyIcon;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public TraySettings()
        {
            KeyboardWidth = KeyboardConstants.MaxColumns;
            KeyboardHeight = KeyboardConstants.MaxRows;
            LoadConfig();
            var trayThread = new Thread(InitializeTray);
            trayThread.Start();
        }

        /// <summary>
        /// Loads stored config values from storage.
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                Tickrate = Math.Abs(Settings.Default.tickrate);
                Saturation = Settings.Default.saturation;
                var keyboardHeightProperty = Settings.Default.keyboardHeight;
                var keyboardWidthProperty = Settings.Default.keyboardWidth;
                AutostartEnabled = Settings.Default.autostartEnabled;
                SelectedMonitor = Settings.Default.monitor;
               
                
                if (keyboardWidthProperty >= 0 && keyboardWidthProperty < KeyboardConstants.MaxColumns)
                {                   
                    KeyboardWidth = keyboardWidthProperty;
                } 
                else
                {
                    _logger.Warn("Invalid keyboardWidth changing back to default value");
                    KeyboardWidth = KeyboardConstants.MaxColumns;
                }

                if (keyboardHeightProperty >= 0 && keyboardHeightProperty < KeyboardConstants.MaxRows)
                {
                    KeyboardHeight = keyboardHeightProperty;
                } 
                else
                {
                    _logger.Warn("Invalid keyboardHeight changing back to default value");
                    KeyboardHeight = KeyboardConstants.MaxRows;
                }
            }
            catch (SettingsPropertyNotFoundException)
            {
                Tickrate = 5;
                Saturation = 1f;
            }

            _logger.Info("Autostart: " + AutostartEnabled);
            _logger.Info("Keyboard width: " + KeyboardWidth);
            _logger.Info("Keyboard height: " + KeyboardHeight);
            _logger.Info("Max FPS: " + Tickrate);
            _logger.Info("Saturation: " + Saturation);   
        }

        /// <summary>
        /// Initializes the tray icons
        /// </summary>
        private void InitializeTray()
        {            
            var keyboardEnabled = new MenuItem("Keyboard enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender);
                Settings.Default.keyboardEnabled = ((MenuItem) sender).Checked;
                KeyboardEnabled = ((MenuItem) sender).Checked;
                Settings.Default.Save();
            });

            // MenuItem _mouseEnabled = new MenuItem("Mouse enabled", (sender, args) =>
            // {
            //     EnableMenuItemOnClick(sender, args);
            //     Properties.Settings.Default.mouseEnabled = (sender as MenuItem).Checked;
            //     MouseEnabled = (sender as MenuItem).Checked;
            //     Properties.Settings.Default.Save();
            // });

            var linkEnabled = new MenuItem("LinkChroma enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender);
                Settings.Default.linkEnabled = ((MenuItem) sender).Checked;
                LinkEnabled = ((MenuItem) sender).Checked;
                Settings.Default.Save();
            });

            var autostart = new MenuItem("Autostart", (sender, args) =>
            {
                EnableMenuItemOnClick(sender);
                Settings.Default.autostartEnabled = ((MenuItem) sender).Checked;
                ChangeAutoStart();
                AutostartEnabled = ((MenuItem) sender).Checked;
                Settings.Default.Save();
            });

            keyboardEnabled.Checked = Settings.Default.keyboardEnabled;
            KeyboardEnabled = Settings.Default.keyboardEnabled;
            // _mouseEnabled.Checked = Properties.Settings.Default.mouseEnabled;
            MouseEnabled = false; //Properties.Settings.Default.mouseEnabled;
            linkEnabled.Checked = Settings.Default.linkEnabled;
            LinkEnabled = Settings.Default.linkEnabled;
            autostart.Checked = CheckAutostart(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ambilight.lnk");
            AutostartEnabled = Settings.Default.autostartEnabled;

            var components = new Container();
            var contextMenu = new ContextMenu();

            contextMenu.MenuItems.Add("Change max fps", ChangeTickrateHandler);
            contextMenu.MenuItems.Add("Change Saturation", ChangeSaturationHandler);
            contextMenu.MenuItems.Add("Set Manual keyboard size", ChangeKeyboardSizeHandler);
            contextMenu.MenuItems.Add("Change Monitor", ChangeMonitorHandler);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(autostart);
            contextMenu.MenuItems.Add("-");

            contextMenu.MenuItems.Add(keyboardEnabled);
            // contextMenu.MenuItems.Add(_mouseEnabled);
            contextMenu.MenuItems.Add(linkEnabled);
            
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Exit", (sender, args) => { _notifyIcon.Dispose();Environment.Exit(0); });


             _notifyIcon = new NotifyIcon(components)
            {
                Icon = new Icon("Color_Wheel.ico"),
                Text = "Razer Ambilight",
                Visible = true
            };

            _logger.Info("Keyboard Enabled: " + keyboardEnabled.Checked);
            // logger.Info("Mouse Enabled: " + _mouseEnabled.Checked);
            _logger.Info("ChromaLink Enabled: " + linkEnabled.Checked);

            _notifyIcon.ContextMenu = contextMenu;
            Application.Run();
        }

        private void ChangeMonitorHandler(object sender, EventArgs e)
        {
            var monitorWindow = new Monitor(MonitorChangedHandler,Settings.Default.monitor);
            monitorWindow.Show();
        }
        private void MonitorChangedHandler(object sender, EventArgs e)
        {
            Settings.Default.monitor = ((ComboBox)sender).SelectedIndex;
            Settings.Default.Save();
            var result=MessageBox.Show("The application must be restarted to apply this change. Do you want to restart now ?", "Restart required", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _notifyIcon.Dispose();
                Process.Start(Application.ExecutablePath);
                Environment.Exit(0);
            }
                
        }

        /// <summary>
        /// Enables a MenuItem to be checkable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void EnableMenuItemOnClick(object sender)
        {
            if (sender is MenuItem item)
            {
                item.Checked = !item.Checked;
            }
        }
        
        /// <summary>
        /// Enables or disables autostart
        /// </summary>
        /// <returns>True if autostart got enabled. False if autostart got disabled</returns>
        private static void ChangeAutoStart()
        {
            var shortcutPath= Environment.GetFolderPath(Environment.SpecialFolder.Startup)+"/Ambilight.lnk";
            if(CheckAutostart(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
            else
            {
                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ambilight.lnk");
                shortcut.Description = "Ambilight for Razer devices";
                shortcut.TargetPath= Path.GetDirectoryName(Application.ExecutablePath)+"/Ambilight.exe";
                shortcut.WorkingDirectory= Path.GetDirectoryName(Application.ExecutablePath);
                shortcut.Save();
            }
        }
        
        /// <summary>
        /// Checks if autostart is enabled or not
        /// </summary>
        /// <param name="shortcutPath">The Filepath of the shortcut</param>
        /// <returns>True if autostart is enabled. False if autostart is not enabled</returns>
        private static bool CheckAutostart(string shortcutPath)
        {
            return File.Exists(shortcutPath);
        }
        
        private void ChangeKeyboardSizeHandler(object sender, EventArgs e)
        {
            var k = new KeyboardSizeControl(KeyboardSizeChangedHandler, KeyboardWidth, KeyboardHeight);
            k.Show();
        }

        private void KeyboardSizeChangedHandler(object sender, EventArgs e)
        {
            var k = sender as KeyboardSizeControl;
            var keyboardWidthSetting = k.GetTxtWidth();
            var keyboardHeightSetting = k.GetTxtHeight();

            if (keyboardWidthSetting < 0 || keyboardWidthSetting > KeyboardConstants.MaxColumns || keyboardHeightSetting < 0 || keyboardHeightSetting > KeyboardConstants.MaxRows)
            {
                k.ErrorReport("Input invalid");
                return;
            }

            KeyboardHeight = keyboardHeightSetting;
            KeyboardWidth = keyboardWidthSetting;
            
            Settings.Default.keyboardWidth = keyboardWidthSetting;
            Settings.Default.keyboardHeight = keyboardHeightSetting;
            Settings.Default.Save();
        }

        /// <summary>
        /// Enables the user to manually change the saturation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeSaturationHandler(object sender, EventArgs e)
        {
            var c = new SaturationControl(SaturationChangedHandler, Saturation * 100);
            c.ShowDialog();
        }

        private void SaturationChangedHandler(object sender, EventArgs e)
        {
            var trackBar = (TrackBar)sender;
            float value = trackBar.Value;
            Saturation = value / 100f;
            Settings.Default.saturation = Saturation;
            Settings.Default.Save();
        }

        /// <summary>
        /// Enables the user to manually change the tickrate with the trayicon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTickrateHandler(object sender, EventArgs e)
        {
            var tickrateString = Interaction.InputBox("New max fps", "change max fps", Tickrate.ToString());
            var newTickrate = Tickrate;
            if (!int.TryParse(tickrateString, out newTickrate) || newTickrate < 1 || newTickrate > 200)
            {
                MessageBox.Show("Invalid input.", "Eror", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Tickrate = newTickrate;
            Settings.Default["tickrate"] = Tickrate;
            Settings.Default.Save();
        }
    }
}
