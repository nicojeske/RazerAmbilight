using IWshRuntimeLibrary;
using Microsoft.VisualBasic;
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
using System.Windows.Forms;
using Colore.Effects.Keyboard;

namespace Ambilight.GUI
{
    /// <summary>
    /// This class handles the settings, as well as the tray icon
    /// </summary>
    public class TraySettings
    {
        public int Tickrate { get; private set; }
        public float Saturation { get; private set; }
        public int KeyboardWidth { get; private set; }
        public int KeyboardHeight { get; private set; }
        public bool KeyboardEnabled { get; private set; }
        public bool MouseEnabled { get; private set; }
        public bool LinkEnabled { get; private set; }
        public bool PadEnabled { get; private set; }
        public bool HeadsetEnabled { get; private set; }
        public bool KeypadEnabeled { get; private set; }
        public bool AmbiModeEnabled { get; private set; }
        public bool UltrawideModeEnabled { get; private set; }
        public bool AutostartEnabled { get; private set; }
        public int SelectedMonitor { get; set; }

        private NotifyIcon notifyIcon;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();


        public TraySettings()
        {
            KeyboardWidth = KeyboardConstants.MaxColumns;
            KeyboardHeight = KeyboardConstants.MaxRows;
            loadConfig();
            Thread trayThread = new Thread(InitializeTray);
            trayThread.Start();
        }

        /// <summary>
        /// Loads stored config values from storage.
        /// </summary>
        private void loadConfig()
        {
            try
            {
                Tickrate = Math.Abs(Properties.Settings.Default.tickrate);
                Saturation = Properties.Settings.Default.saturation;
                int _keyboardHeightProperty = Properties.Settings.Default.keyboardHeight;
                int _keyboardWidthProperty = Properties.Settings.Default.keyboardWidth;
                AutostartEnabled = Properties.Settings.Default.autostartEnabled;
                SelectedMonitor = Properties.Settings.Default.monitor;
               


                if (_keyboardWidthProperty >= 0 && _keyboardWidthProperty < KeyboardConstants.MaxColumns)
                {                   
                    KeyboardWidth = _keyboardWidthProperty;
                } else
                {
                    logger.Warn("Invalid keyboardWidth changing back to default value");
                    KeyboardWidth = KeyboardConstants.MaxColumns;
                }

                if (_keyboardHeightProperty >= 0 && _keyboardHeightProperty < KeyboardConstants.MaxRows)
                {
                    KeyboardHeight = _keyboardHeightProperty;
                } else
                {
                    logger.Warn("Invalid keyboardHeight changing back to default value");
                    KeyboardHeight = KeyboardConstants.MaxRows;
                }
            }
            catch (SettingsPropertyNotFoundException)
            {
                Tickrate = 5;
                Saturation = 1f;
            }

            logger.Info("Autostart: " + AutostartEnabled);
            logger.Info("Keyboard width: " + KeyboardWidth);
            logger.Info("Keyboard height: " + KeyboardHeight);
            logger.Info("Max FPS: " + Tickrate);
            logger.Info("Saturation: " + Saturation);   
        }

        /// <summary>
        /// Initializes the tray icons
        /// </summary>
        private void InitializeTray()
        {            
            MenuItem _keyboardEnabled = new MenuItem("Keyboard enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.keyboardEnabled = (sender as MenuItem).Checked;
                KeyboardEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            MenuItem _mouseEnabled = new MenuItem("Mouse enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.mouseEnabled = (sender as MenuItem).Checked;
                MouseEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            MenuItem _mousematEnabled = new MenuItem("Mousemat enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.mousematEnabled = (sender as MenuItem).Checked;
                PadEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });
            
            MenuItem _headsetEnabled = new MenuItem("Headset enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.headsetEnabled = (sender as MenuItem).Checked;
                HeadsetEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });
            
            MenuItem _keypadEnabled = new MenuItem("Keypad enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.keypadEnabled = (sender as MenuItem).Checked;
                KeypadEnabeled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            MenuItem _linkEnabled = new MenuItem("LinkChroma enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.linkEnabled = (sender as MenuItem).Checked;
                LinkEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            MenuItem _ambiModeEnabled = new MenuItem("'Real' Ambilight mode", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.ambiEnabled = (sender as MenuItem).Checked;
                AmbiModeEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            MenuItem _ultrawideModeEnabled = new MenuItem("Ultrawide Monitor mode", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.ultrawideEnabled = (sender as MenuItem).Checked;
                UltrawideModeEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            MenuItem _autostart = new MenuItem("Autostart", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.autostartEnabled = (sender as MenuItem).Checked;
                changeAutoStart();
                AutostartEnabled = (sender as MenuItem).Checked;
                Properties.Settings.Default.Save();
            });

            _keyboardEnabled.Checked = Properties.Settings.Default.keyboardEnabled;
            KeyboardEnabled = Properties.Settings.Default.keyboardEnabled;
            _mouseEnabled.Checked = Properties.Settings.Default.mouseEnabled;
            MouseEnabled = Properties.Settings.Default.mouseEnabled;
            _mousematEnabled.Checked = Properties.Settings.Default.mousematEnabled;
            PadEnabled = Properties.Settings.Default.mousematEnabled;
            _headsetEnabled.Checked = Properties.Settings.Default.headsetEnabled;
            HeadsetEnabled = Properties.Settings.Default.headsetEnabled;
            _keypadEnabled.Checked = Properties.Settings.Default.keypadEnabled;
            KeypadEnabeled = Properties.Settings.Default.keypadEnabled;
            _linkEnabled.Checked = Properties.Settings.Default.linkEnabled;
            LinkEnabled = Properties.Settings.Default.linkEnabled;
            _ambiModeEnabled.Checked = Properties.Settings.Default.ambiEnabled;
            AmbiModeEnabled = Properties.Settings.Default.ambiEnabled;
            _ultrawideModeEnabled.Checked = Properties.Settings.Default.ambiEnabled;
            UltrawideModeEnabled = Properties.Settings.Default.ultrawideEnabled;
            _autostart.Checked = checkAutostart(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ambilight.lnk");
            AutostartEnabled = Properties.Settings.Default.autostartEnabled;

            var components = new System.ComponentModel.Container();
            var contextMenu = new ContextMenu();

            contextMenu.MenuItems.Add("Change max fps", ChangeTickrateHandler);
            contextMenu.MenuItems.Add("Change Saturation", ChangeSaturationHandler);
            contextMenu.MenuItems.Add("Set Manual keyboard size", changeKeyboardSizeHandler);
            contextMenu.MenuItems.Add("Change Monitor", changeMonitorHandler);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(_ambiModeEnabled);
            contextMenu.MenuItems.Add(_ultrawideModeEnabled);
            contextMenu.MenuItems.Add(_autostart);
            contextMenu.MenuItems.Add("-");

            contextMenu.MenuItems.Add(_keyboardEnabled);
            contextMenu.MenuItems.Add(_mouseEnabled);
            contextMenu.MenuItems.Add(_mousematEnabled);
            contextMenu.MenuItems.Add(_headsetEnabled);
            contextMenu.MenuItems.Add(_keypadEnabled);
            contextMenu.MenuItems.Add(_linkEnabled);
            
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Exit", (sender, args) => { notifyIcon.Dispose();Environment.Exit(0); });


             notifyIcon = new NotifyIcon(components)
            {
                Icon = new Icon("Color_Wheel.ico"),
                Text = "Razer Ambilight",
                Visible = true
            };

            logger.Info("Keyboard Enabled: " + _keyboardEnabled.Checked);
            logger.Info("Mouse Enabled: " + _mouseEnabled.Checked);
            logger.Info("Mousemat Enabled: " + _mousematEnabled.Checked);
            logger.Info("Headset Enabled: " + _headsetEnabled.Checked);
            logger.Info("Keypad Enabled: " + _keypadEnabled.Checked);
            logger.Info("ChromaLink Enabled: " + _linkEnabled.Checked);
            logger.Info("Ambilight mode: " + _ambiModeEnabled.Checked);
            logger.Info("Ultrawide mode: " + _ultrawideModeEnabled.Checked);

            notifyIcon.ContextMenu = contextMenu;
            Application.Run();
        }

        private void changeMonitorHandler(object sender, EventArgs e)
        {
            Monitor monitorWindow = new Monitor(monitorChangedHandler,Properties.Settings.Default.monitor);
            monitorWindow.Show();
        }
        private void monitorChangedHandler(object sender, EventArgs e)
        {
            Properties.Settings.Default.monitor = ((ComboBox)sender).SelectedIndex;
            Properties.Settings.Default.Save();
            DialogResult result=MessageBox.Show("The application must be restarted to apply this change. Do you want to restart now ?", "Restart required", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                notifyIcon.Dispose();
                System.Diagnostics.Process.Start(Application.ExecutablePath);
                Environment.Exit(0);
            }
                
        }

        /// <summary>
        /// Enables a MenuItem to be checkable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void EnableMenuItemOnClick(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            item.Checked = !item.Checked;
        }
        /// <summary>
        /// Enables or disables autostart
        /// </summary>
        /// <returns>True if autostart got enabled. False if autostart got disabled</returns>
        private bool changeAutoStart()
        {
            string shortcutPath= Environment.GetFolderPath(Environment.SpecialFolder.Startup)+"/Ambilight.lnk";
            if(checkAutostart(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
                return false;
            }
            else
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/Ambilight.lnk");
                shortcut.Description = "Ambilight for Razer devices";
                shortcut.TargetPath= System.IO.Path.GetDirectoryName(Application.ExecutablePath)+"/Ambilight.exe";
                shortcut.WorkingDirectory= System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                shortcut.Save();
                return true;
            }
            

        }
        /// <summary>
        /// Checks if autostart is enabled or not
        /// </summary>
        /// <param name="shortcutPath">The Filepath of the shortcut</param>
        /// <returns>True if autostart is enabled. False if autostart is not enabled</returns>
        private bool checkAutostart(string shortcutPath)
        {
            if (System.IO.File.Exists(shortcutPath))
                return true;
            else
                return false;
        }
        private void changeKeyboardSizeHandler(object sender, EventArgs e)
        {
            KeyboardSizeControl k = new KeyboardSizeControl(keyboardSizeChangedHandler, KeyboardWidth, KeyboardHeight);
            k.Show();
        }

        private void keyboardSizeChangedHandler(object sender, EventArgs e)
        {
            KeyboardSizeControl k = sender as KeyboardSizeControl;
            int KeyboardWidthSetting = k.GetTxtWidth();
            int KeyboardHeightSetting = k.GetTxtHeight();

            if (KeyboardWidthSetting < 0 || KeyboardWidthSetting > KeyboardConstants.MaxColumns || KeyboardHeightSetting < 0 || KeyboardHeightSetting > KeyboardConstants.MaxRows)
            {
                k.errorReport("Input invalid");
                return;
            }

            KeyboardHeight = KeyboardHeightSetting;
            KeyboardWidth = KeyboardWidthSetting;


            Properties.Settings.Default.keyboardWidth = KeyboardWidthSetting;
            Properties.Settings.Default.keyboardHeight = KeyboardHeightSetting;
            Properties.Settings.Default.Save();

        }

        /// <summary>
        /// Enables the user to manually change the saturation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeSaturationHandler(object sender, EventArgs e)
        {
            SaturationControl c = new SaturationControl(SaturationChangedHandler, Saturation * 100);
            c.ShowDialog();
        }

        private void SaturationChangedHandler(object sender, EventArgs e)
        {
            var trackBar = (TrackBar)sender;
            float value = trackBar.Value;
            Saturation = value / 100f;
            Properties.Settings.Default.saturation = Saturation;
            Properties.Settings.Default.Save();
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
            Properties.Settings.Default["tickrate"] = Tickrate;
            Properties.Settings.Default.Save();
        }
    }
}
