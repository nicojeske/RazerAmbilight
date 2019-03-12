using Microsoft.VisualBasic;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public bool KeyboardEnabledBool { get; private set; }
        public bool MouseEnabledBool { get; private set; }
        public bool PadEnabledBool { get; private set; }
       
        private readonly Logger logger = LogManager.GetCurrentClassLogger();


        public TraySettings()
        {
            KeyboardWidth = Corale.Colore.Razer.Keyboard.Constants.MaxColumns;
            KeyboardHeight = Corale.Colore.Razer.Keyboard.Constants.MaxRows;
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



                if (_keyboardWidthProperty >= 0)
                {
                    KeyboardWidth = _keyboardWidthProperty;
                }

                if (_keyboardHeightProperty >= 0)
                {
                    KeyboardHeight = _keyboardHeightProperty;
                }
            }
            catch (SettingsPropertyNotFoundException)
            {
                Tickrate = 5;
                Saturation = 1f;
            }

            

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
                   KeyboardEnabledBool = (sender as MenuItem).Checked;
                   Properties.Settings.Default.Save();
               });


            MenuItem _mouseEnabled = new MenuItem("Mouse enabled", (sender, args) =>
           {
               EnableMenuItemOnClick(sender, args);
               Properties.Settings.Default.mouseEnabled = (sender as MenuItem).Checked;
               MouseEnabledBool = (sender as MenuItem).Checked;
               Properties.Settings.Default.Save();
           });



            MenuItem _mousematEnabled = new MenuItem("Mousemat enabled", (sender, args) =>
           {
               EnableMenuItemOnClick(sender, args);
               Properties.Settings.Default.mousematEnabled = (sender as MenuItem).Checked;
               PadEnabledBool = (sender as MenuItem).Checked;
               Properties.Settings.Default.Save();
           });

            _keyboardEnabled.Checked = Properties.Settings.Default.keyboardEnabled;
            KeyboardEnabledBool = Properties.Settings.Default.keyboardEnabled;
            _mouseEnabled.Checked = Properties.Settings.Default.mouseEnabled;
            MouseEnabledBool = Properties.Settings.Default.mouseEnabled;
            _mousematEnabled.Checked = Properties.Settings.Default.mousematEnabled;
            PadEnabledBool = Properties.Settings.Default.mousematEnabled;


            var components = new System.ComponentModel.Container();
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Exit", (sender, args) => Environment.Exit(0));
            contextMenu.MenuItems.Add("Change max fps", ChangeTickrateHandler);
            contextMenu.MenuItems.Add("Change Saturation", ChangeSaturationHandler);
            contextMenu.MenuItems.Add("Set Manual keyboard size", changeKeyboardSizeHandler);
            contextMenu.MenuItems.Add("-");

            contextMenu.MenuItems.Add(_keyboardEnabled);
            contextMenu.MenuItems.Add(_mouseEnabled);
            contextMenu.MenuItems.Add(_mousematEnabled);

            var notifyIcon = new NotifyIcon(components)
            {
                Icon = new Icon("Color_Wheel.ico"),
                Text = "Razer Ambilight",
                Visible = true
            };

            notifyIcon.ContextMenu = contextMenu;
            Application.Run();
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


        
        private void changeKeyboardSizeHandler(object sender, EventArgs e)
        {
            KeyboardSizeControl k = new KeyboardSizeControl(keyboardSizeChangedHandler, KeyboardWidth, KeyboardHeight);
            k.Show();
        }

        private void keyboardSizeChangedHandler(object sender, EventArgs e)
        {
            KeyboardSizeControl k = sender as KeyboardSizeControl;
            KeyboardWidth = k.GetTxtWidth();
            KeyboardHeight = k.GetTxtHeight();
            Properties.Settings.Default.keyboardWidth = KeyboardWidth;
            Properties.Settings.Default.keyboardHeight = KeyboardHeight;
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
