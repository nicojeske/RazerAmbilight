using NLog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace Ambilight.DesktopDuplication
{
    internal class DesktopDuplicatorReader : IDesktopDuplicatorReader
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Logic.LogicManager _logic;
        private readonly GUI.TraySettings settings;

        public DesktopDuplicatorReader(Logic.LogicManager logic, GUI.TraySettings settings)
        {
            this._logic = logic;
            this.settings = settings;


            RefreshCapturingState();

            _log.Info($"DesktopDuplicatorReader created.");
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
                    Priority = ThreadPriority.Normal,
                    Name = "DesktopDuplicatorReader"
                };
                thread.Start();
            }
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
                    var newImage = GetNextFrame(image);
                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;

                    _logic.ProcessNewImage(newImage);

                    int minFrameTimeInMs = 1; //1000/FPS
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

    
      

        private Bitmap GetNextFrame(Bitmap reusableBitmap)
        {
            if (_desktopDuplicator == null)
            {
                _desktopDuplicator = new DesktopDuplicator(0, settings.SelectedMonitor);
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
                return null;
            }
        }

      

    }
}
