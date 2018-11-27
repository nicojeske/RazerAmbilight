using Corale.Colore.Core;
using Microsoft.VisualBasic;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using Color = System.Drawing.Color;
using ColoreColor = Corale.Colore.Core.Color;
using Constants = Corale.Colore.Razer.Keyboard.Constants;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;

namespace Ambilight
{
    internal class Program
    {
        private static int _tickrate;
        private static float _saturation;

        private static void Main(string[] args)
        {

            AutoUpdater.Start("https://vertretungsplan.ga/ambi/ambi.xml");

            try
            {
                _tickrate = Math.Abs(Properties.Settings.Default.tickrate);
                _saturation = Properties.Settings.Default.saturation;
            }
            catch (SettingsPropertyNotFoundException)
            {
                _tickrate = 5;
                _saturation = 1f;
            }

            Console.WriteLine("tickrate: " + _tickrate);

            if (args.Length == 1)
            {
                int.TryParse(args[0], out _tickrate);
            }

            //Initializing Chroma SDK
            Chroma.Instance.Initialize();

            //Initialize Tray
            var trayThread = new Thread(InitializeTray);
            trayThread.Start();

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
                    //For now ignore exceptions. And just try again in a second.
                    //TODO: Implement a better way to check for errors and maybe use some method to send error codes to my server for debugging purposes.
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Creates a tray item, which enables the user to close the application
        /// </summary>
        private static void InitializeTray()
        {
            var components = new System.ComponentModel.Container();
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Exit", (sender, args) => Environment.Exit(0));
            contextMenu.MenuItems.Add("Change tickrate", ChangeTickrateHandler);
            contextMenu.MenuItems.Add("Change Saturation", ChangeSaturationHandler);

            var notifyIcon = new NotifyIcon(components)
            {
                Icon = new Icon("Color_Wheel.ico"),
                Text = "Razer Ambilight",
                Visible = true,
            };

            notifyIcon.ContextMenu = contextMenu;
            Application.Run();
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

        /// <summary>
        /// Resize an image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.Bicubic;
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.PixelOffsetMode = PixelOffsetMode.None;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
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

            //Resizing the screenshot to the layout of the keyboard.
            Bitmap map = ResizeImage(screen, Constants.MaxColumns, Constants.MaxRows);

            map = ApplySaturation(_saturation, map);

            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < Constants.MaxRows; r++)
            {
                for (var c = 0; c < Constants.MaxColumns; c++)
                {
                    Color color = map.GetPixel(c, r);

                    keyboardGrid[r, c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }

            Bitmap mapMouse = ResizeImage(screen, Corale.Colore.Razer.Mouse.Constants.MaxColumns,
                Corale.Colore.Razer.Mouse.Constants.MaxRows);

            mapMouse = ApplySaturation(_saturation, mapMouse);

            for (var r = 0; r < Corale.Colore.Razer.Mouse.Constants.MaxRows; r++)
            {
                for (var c = 0; c < Corale.Colore.Razer.Mouse.Constants.MaxColumns; c++)
                {
                    Color color = mapMouse.GetPixel(c, r);
                    mouseGrid[r, c] = new ColoreColor((byte) color.R, (byte) color.G, (byte) color.B);
                }
            }

            Bitmap mapMousePad = ResizeImage(screen, 6,4);
            mapMousePad = ApplySaturation(_saturation, mapMouse);

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
                mousePadGrid[10-i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }

            for (int i = 3; i >= 0; i--)
            {
                Color color = mapMousePad.GetPixel(0, i);
                mousePadGrid[14 - i] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
            }


            //The custom effect from the keyboard grid is applied to the keyboard.
            Chroma.Instance.Keyboard.SetCustom(keyboardGrid);
            Chroma.Instance.Mouse.SetGrid(mouseGrid);
            Chroma.Instance.Mousepad.SetCustom(mousePadGrid);

            //The graphic object, as well as the screenshot are disposed
            gfxScreenshot.Dispose();
            screen.Dispose();
            mapMouse.Dispose();
            map.Dispose();
        }

        private static Bitmap ApplySaturation(float saturation, Bitmap srcBitmap)
        {
            float rWeight = 0.3086f;
            float gWeight = 0.6094f;
            float bWeight = 0.0820f;

            float a = (1.0f - saturation) * rWeight + saturation;
            float b = (1.0f - saturation) * rWeight;
            float c = (1.0f - saturation) * rWeight;
            float d = (1.0f - saturation) * gWeight;
            float e = (1.0f - saturation) * gWeight + saturation;
            float f = (1.0f - saturation) * gWeight;
            float g = (1.0f - saturation) * bWeight;
            float h = (1.0f - saturation) * bWeight;
            float i = (1.0f - saturation) * bWeight + saturation;

            Bitmap returnBitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height);

            // Create a Graphics
            using (Graphics gr = Graphics.FromImage(returnBitmap))
            {
                // ColorMatrix elements
                float[][] ptsArray = {
                                     new float[] {a,  b,  c,  0, 0},
                                     new float[] {d,  e,  f,  0, 0},
                                     new float[] {g,  h,  i,  0, 0},
                                     new float[] {0,  0,  0,  1, 0},
                                     new float[] {0, 0, 0, 0, 1}
                                 };
                // Create ColorMatrix
                ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
                // Create ImageAttributes
                ImageAttributes imgAttribs = new ImageAttributes();
                // Set color matrix
                imgAttribs.SetColorMatrix(clrMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Default);
                // Draw Image with no effects
                gr.DrawImage(srcBitmap, 0, 0, 200, 200);
                // Draw Image with image attributes
                gr.DrawImage(srcBitmap,
                    new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height),
                    0, 0, srcBitmap.Width, srcBitmap.Height,
                    GraphicsUnit.Pixel, imgAttribs);
            }

            return returnBitmap;
        }
    }
}