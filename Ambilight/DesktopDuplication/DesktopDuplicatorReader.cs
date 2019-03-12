using NLog;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ambilight.DesktopDuplication
{
    internal class DesktopDuplicatorReader : IDesktopDuplicatorReader
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly Program _logic;


        public DesktopDuplicatorReader(Program logic)
        {
            this._logic = logic;
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);

            RefreshCapturingState();

            _log.Info($"DesktopDuplicatorReader created.");
        }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;


        private void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;

            if (isRunning)
            {
                //stop it!
                _log.Debug("stopping the capturing");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning)
            {
                //start it
                _log.Debug("starting the capturing");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                thread.Start();
            }
        }

        private readonly Policy _retryPolicy;

        private TimeSpan ProvideDelayDuration(int index)
        {
            if (index < 10)
            {
                //first second
                return TimeSpan.FromMilliseconds(100);
            }

            if (index < 10 + 256)
            {
                //steps where there is also led dimming
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }

        private DesktopDuplicator _desktopDuplicator;

        public void Run(CancellationToken token)
        {
            if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            IsRunning = true;
            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            try
            {
                BitmapData bitmapData = new BitmapData();

                while (!token.IsCancellationRequested)
                {
                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image));
                    TraceFrameDetails(newImage);
                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;

                    _logic.newImage(newImage);



                    int minFrameTimeInMs = 1000 / 20; //1000/FPS
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                }
            }
            finally
            {
                image?.Dispose();

                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;

                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;
            }
        }

        private int? _lastObservedHeight;
        private int? _lastObservedWidth;

        private void TraceFrameDetails(Bitmap image)
        {
            //there are many frames per second and we need to extract useful information and only log those!
            if (image == null)
            {
                //if the frame is null, this can mean two things. the timeout from the desktop duplication api was reached
                //before the monitor content changed or there was some other error.
            }
            else
            {
                if (_lastObservedHeight != null && _lastObservedWidth != null
                    && (_lastObservedHeight != image.Height || _lastObservedWidth != image.Width))
                {
                    _log.Debug("The frame size changed from {0}x{1} to {2}x{3}"
                        , _lastObservedWidth, _lastObservedHeight
                        , image.Width, image.Height);

                }
                _lastObservedWidth = image.Width;
                _lastObservedHeight = image.Height;
            }
        }

        private void ApplyColorCorrections(float r, float g, float b, out byte finalR, out byte finalG, out byte finalB, bool useLinearLighting, byte saturationTreshold
            , byte lastColorR, byte lastColorG, byte lastColorB)
        {
          
                //output
                finalR = (byte)r;
                finalG = (byte)g;
                finalB = (byte)b;
            
        }

        private Bitmap GetNextFrame(Bitmap reusableBitmap)
        {
            if (_desktopDuplicator == null)
            {
                _desktopDuplicator = new DesktopDuplicator(0, 0);
            }

            try
            {
                return _desktopDuplicator.GetLatestFrame(reusableBitmap);
            }
            catch (Exception ex)
            {
                if (ex.Message != "_outputDuplication is null")
                {
                    _log.Error(ex, "GetNextFrame() failed.");
                }

                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;
                throw;
            }
        }

      

    }
}
