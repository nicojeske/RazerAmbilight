﻿using System;
using Corale.Colore.Core;
using ColoreColor = Corale.Colore.Core.Color;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Corale.Colore.Razer.Keyboard;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;

namespace Ambilight
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tickrate
            var tickrate = 5;

            if (args.Length == 1)
                int.TryParse(args[0], out tickrate);

            //Initializing Chroma SDK
            Chroma.Instance.Initialize();

            //Initialize Tray
            Thread trayThread = new Thread(InitializeTray);
            trayThread.Start();

            //Update every x ms since last update.
            while (true)
            {
                try
                {
                    UpdateAmbiligth();
                    Thread.Sleep(tickrate);
                }
                catch (Exception e)
                {
                    MessageBox.Show("It seems Ambilight doesn't work for you. Sorry for the inconvenience. Errormessage: " +
                                    e.Message);
                    return;
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
            contextMenu.MenuItems.Add("Exit", (sender, args) =>  Environment.Exit(0));

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
            var map = ResizeImage(screen, Constants.MaxColumns, Constants.MaxRows);

            //Iterating over each key and set it to the corrosponding color of the resized Screenshot
            for (var r = 0; r < Constants.MaxRows; r++)
            {
                for (var c = 0; c < Constants.MaxColumns; c++)
                {
                    var color = map.GetPixel(c, r);
                    keyboardGrid[r,c] = new ColoreColor((byte)color.R, (byte)color.G, (byte)color.B);
                }
            }

            //The custom effect from the keyboard grid is applied to the keyboard.
            Chroma.Instance.Keyboard.SetCustom(keyboardGrid);

            //The graphic object, as well as the screenshot are disposed
            gfxScreenshot.Dispose();
            screen.Dispose();
            map.Dispose();
        }
    }
}
