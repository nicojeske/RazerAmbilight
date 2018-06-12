using System;
using Corale.Colore.Core;
using ColoreColor = Corale.Colore.Core.Color;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Corale.Colore.Razer.Keyboard;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;

namespace Ambilight
{
    internal class Program
    {
        //RNG
        private static Random _rng = new Random();
        

        static void Main(string[] args)
        {
            //Accuracy (lower = better, but slower)
            var accuracy = 10;
            //Tickrate
            var tickrate = 1;

            switch (args.Length)
            {
                case 1:
                    accuracy = int.Parse(args[0]);
                    break;
                case 2:
                    accuracy = int.Parse(args[0]);
                    tickrate = int.Parse(args[1]);
                    break;
            }

            //Initializing Chroma SDK
            Chroma.Instance.Initialize();

            //Tick count for updating Ambiligth
            var start = Environment.TickCount;

            //Update every x ms since last update.
            while (true)
            {
                UpdateAmbiligth(accuracy);
                Thread.Sleep(tickrate);
            }
            
        }

        private static void UpdateAmbiligth(int accuracy)
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

            //Iterating over each key, to calculate and set the wished color.
            for (var r = 0; r < Constants.MaxRows; r++)
            {
                for (var c = 0; c < Constants.MaxColumns; c++)
                {
                    //Imaginary splitting the image into blocks of equal size, which corresponds to the position of the current key.
                    //e.g. for a screen resolution of 1920*1080 pixels, the pixel range x: 0 - approx. 90 and y: 0 - approx. 150 is considered for the ESC key.
                    long red = 0;
                    long green = 0;
                    long blue = 0;
                    long count = 0;

                    //Calculate the size of the blocks depending on the number of keys on the keyboard
                    var blockHeigth = (Screen.PrimaryScreen.Bounds.Height / Constants.MaxRows);
                    var blockWidth = (Screen.PrimaryScreen.Bounds.Width / Constants.MaxColumns);

                    //A certain number of pixels in this range are considered and thus finally the average color value for this range is determined.
                    //The Accuracy variable determines how many pixels are to be skipped and is therefore decisive for speed and processor utilization.
                    for (var x = 0; x < blockHeigth; x+=accuracy)
                    {
                        for(var y = 0; y < blockWidth; y+=accuracy)
                        {
                            var pixelX = c * blockWidth + x;
                            var pixelY = r * blockHeigth + y;
                            //Since the block size calculation may require rounding, it is necessary to check that the pixel coordinates are not outside the image.
                            if (pixelX >= Screen.PrimaryScreen.Bounds.Width)
                                pixelX = Screen.PrimaryScreen.Bounds.Width-1;
                            if (pixelY >= Screen.PrimaryScreen.Bounds.Height)
                                pixelY = Screen.PrimaryScreen.Bounds.Height - 1;

                            //Get the pixel from the given coordinates
                            var pixel = screen.GetPixel(pixelX, pixelY);
                            //Adding up the color values to be able to calculate the average color of the block later.
                            red += pixel.R;
                            green += pixel.G;
                            blue += pixel.B;
                            count++;
                        }
                    }

                    //Calculate the average color values for this block
                    var avgR = (int) (red / count);
                    var avgG = (int) (green / count);
                    var avgB = (int) (blue / count);

                    //Saving the calculated average color in the grid of the keyboard
                    keyboardGrid[r, c] = new ColoreColor((byte)avgR, (byte)avgG, (byte)avgB);
                }
            }

            //The custom effect from the keyboard grid is applied to the keyboard.
            Chroma.Instance.Keyboard.SetCustom(keyboardGrid);

            //The graphic object, as well as the screenshot are disposed
            gfxScreenshot.Dispose();
            screen.Dispose();
        }

    

      
        
    }
}
