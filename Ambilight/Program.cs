using AutoUpdaterDotNET;
using Corale.Colore.Core;
using Corale.Colore.Razer.Mouse.Effects;
using Microsoft.VisualBasic;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using NLog;
using Color = System.Drawing.Color;
using ColoreColor = Corale.Colore.Core.Color;
using Custom = Corale.Colore.Razer.Mousepad.Effects.Custom;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;

namespace Ambilight
{


    internal class Program
    {
        private static int _tickrate;
        private static float _saturation;
        private static MenuItem _keyboardEnabled;
        private static MenuItem _mouseEnabled;
        private static MenuItem _mousematEnabled;
        private static int _keyboardWidth = Corale.Colore.Razer.Keyboard.Constants.MaxColumns;
        private static int _keyboardHeight = Corale.Colore.Razer.Keyboard.Constants.MaxRows;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            AutoUpdater.Start("https://vertretungsplan.ga/ambi/ambi.xml");

            loadConfig();

            //Initializing Chroma SDK
            Chroma.Instance.Initialize();

            //Initialize Tray
            var trayThread = new Thread(InitializeTray);
            trayThread.Start();

            logger.Info("Initialized");

            //Update every x ms since last update.
            while (true)
            {
                try
                {
                    UpdateAmbiligth();
                    Thread.Sleep(_tickrate);
                }
                catch (Exception e)
                {
                    logger.Warn("----------------ERROR START------------------");
                    logger.Error(e);
                    logger.Warn("---------------- ERROR END ------------------");

                    Thread.Sleep(2000);                    
                }
                
            } 
        }

        private static void loadConfig()
        {

           
            _keyboardEnabled = new MenuItem("Keyboard enabled", (sender, args) =>
                {
                    EnableMenuItemOnClick(sender, args);
                    Properties.Settings.Default.keyboardEnabled = _keyboardEnabled.Checked;
                    Properties.Settings.Default.Save();
                });

            _mouseEnabled = new MenuItem("Mouse enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.mouseEnabled = _mouseEnabled.Checked;
                Properties.Settings.Default.Save();
            });

            _mousematEnabled = new MenuItem("Mousemat enabled", (sender, args) =>
            {
                EnableMenuItemOnClick(sender, args);
                Properties.Settings.Default.mousematEnabled = _mousematEnabled.Checked;
                Properties.Settings.Default.Save();
            });

            try
            {
                _tickrate = Math.Abs(Properties.Settings.Default.tickrate);
                _saturation = Properties.Settings.Default.saturation;
                _keyboardEnabled.Checked = Properties.Settings.Default.keyboardEnabled;
                _mouseEnabled.Checked = Properties.Settings.Default.mouseEnabled;
                _mousematEnabled.Checked = Properties.Settings.Default.mousematEnabled;
                int _keyboardHeightProperty = Properties.Settings.Default.keyboardHeight;
                int _keyboardWidthProperty = Properties.Settings.Default.keyboardWidth;

                if (_keyboardWidthProperty >= 0)
                {
                    _keyboardWidth = _keyboardWidthProperty;
                }

                if (_keyboardHeightProperty >= 0)
                {
                    _keyboardHeight = _keyboardHeightProperty;
                }
            }
            catch (SettingsPropertyNotFoundException)
            {
                _tickrate = 5;
                _saturation = 1f;
            }
        }

        /// <summary>
        /// Creates a tray item, which enables the user to close the application and change settings
        /// </summary>
        private static void InitializeTray()
        {
            var components = new System.ComponentModel.Container();
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Exit", (sender, args) => Environment.Exit(0));
            contextMenu.MenuItems.Add("Change tickrate", ChangeTickrateHandler);
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

        private static void changeKeyboardSizeHandler(object sender, EventArgs e)
        {
            KeyboardSizeControl k = new KeyboardSizeControl(keyboardSizeChangedHandler, _keyboardWidth, _keyboardHeight);
            k.Show();
        }

        private static void keyboardSizeChangedHandler(object sender, EventArgs e)
        {
            KeyboardSizeControl k = sender as KeyboardSizeControl;
            _keyboardWidth = k.GetTxtWidth();
            _keyboardHeight = k.GetTxtHeight();
            Properties.Settings.Default.keyboardWidth = _keyboardWidth;
            Properties.Settings.Default.keyboardHeight = _keyboardHeight;
            Properties.Settings.Default.Save();
        }

        private static void EnableMenuItemOnClick(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            item.Checked = !item.Checked;
        }

        /// <summary>
        /// Enables the user to manually change the saturation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ChangeSaturationHandler(object sender, EventArgs e)
        {
            SaturationControl c = new SaturationControl(SaturationChangedHandler, _saturation * 100);
            c.ShowDialog();
        }

        private static void SaturationChangedHandler(object sender, EventArgs e)
        {
            var trackBar = (TrackBar)sender;
            float value = trackBar.Value;
            _saturation = value / 100f;
            Properties.Settings.Default.saturation = _saturation;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Enables the user to manually change the tickrate with the trayicon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ChangeTickrateHandler(object sender, EventArgs e)
        {
            var tickrateString = Interaction.InputBox("New tickrate in ms", "change tickrate", _tickrate.ToString());
            var newTickrate = _tickrate;
            if (!int.TryParse(tickrateString, out newTickrate) || newTickrate < 1)
            {
                MessageBox.Show("Invalid input.", "Eror", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _tickrate = newTickrate;
            Properties.Settings.Default["tickrate"] = _tickrate;
            Properties.Settings.Default.Save();
        }

        private static void UpdateAmbiligth()
        {
            //Empty CustomGrid to generate the ambilight effect into it.
            var keyboardGrid = KeyboardCustom.Create();
            var mouseGrid = Corale.Colore.Razer.Mouse.Effects.CustomGrid.Create();
            var mousePadGrid = Corale.Colore.Razer.Mousepad.Effects.Custom.Create();

            //Creating a new Bitmap, with the current display resolution.
            var screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                Screen.PrimaryScreen.Bounds.Height,
                                PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(screen);

            // Take the screenshot from the upper left corner to the right bottom corner of the screen.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            //Actual Ambilight feature. Resizing the screencapture for the corresponding device and
            //copy the colors.
  
            if (_keyboardEnabled.Checked)
            {
                Bitmap map = Util.ResizeImage(screen, _keyboardWidth, _keyboardHeight);
                map = Util.ApplySaturation(map, _saturation);
                keyboardGrid = GenerateKeyboardGrid(map, keyboardGrid);
                Chroma.Instance.Keyboard.SetCustom(keyboardGrid);
                map.Dispose();
            }

            if (_mouseEnabled.Checked)
            {
                Bitmap mapMouse = Util.ResizeImage(screen, Corale.Colore.Razer.Mouse.Constants.MaxColumns,
                    Corale.Colore.Razer.Mouse.Constants.MaxRows);
                mapMouse = Util.ApplySaturation(mapMouse, _saturation);
                mouseGrid = GenerateMouseGrid(mapMouse, mouseGrid);
                Chroma.Instance.Mouse.SetGrid(mouseGrid);
                mapMouse.Dispose();
            }

            if (_mousematEnabled.Checked)
            {
                Bitmap mapMousePad = Util.ResizeImage(screen, 7, 6);
                mapMousePad = Util.ApplySaturation(mapMousePad, _saturation);
                mousePadGrid = GenerateMousePadGrid(mapMousePad, mousePadGrid);
                Chroma.Instance.Mousepad.SetCustom(mousePadGrid);
                mapMousePad.Dispose();
            }

            //The graphic object, as well as the screenshot are disposed
            gfxScreenshot.Dispose();
            screen.Dispose();
        }

        private static Custom GenerateMousePadGrid(Bitmap mapMousePad, Custom mousePadGrid)
        {
            for (int i = 0; i < 4; i++)
            {
                Color color = mapMousePad.GetPixel(6, i);
                mousePadGrid[i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }

            Color colorC = mapMousePad.GetPixel(6, 4);
            mousePadGrid[4] = new ColoreColor((byte)colorC.R, (byte)colorC.G, (byte)colorC.B);

            for (int i = 5; i >= 0; i--)
            {
                Color color = mapMousePad.GetPixel(i, 5);
                mousePadGrid[10 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }

            for (int i = 3; i >= 0; i--)
            {
                Color color = mapMousePad.GetPixel(0, i);
                mousePadGrid[14 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }

            return mousePadGrid;
        }

        private static CustomGrid GenerateMouseGrid(Bitmap mapMouse, CustomGrid mouseGrid)
        {
            for (var r = 0; r < Corale.Colore.Razer.Mouse.Constants.MaxRows; r++)
            {
                for (var c = 0; c < Corale.Colore.Razer.Mouse.Constants.MaxColumns; c++)
                {
                    Color color = mapMouse.GetPixel(c, r);
                    mouseGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }

            return mouseGrid;
        }

        private static KeyboardCustom GenerateKeyboardGrid(Bitmap map, KeyboardCustom keyboardGrid)
        {
            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < _keyboardHeight; r++)
            {
                for (var c = 0; c < _keyboardWidth; c++)
                {
                    Color color = map.GetPixel(c, r);

                    keyboardGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }

            return keyboardGrid;
        }
    }
}