using NLog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Ambilight.DesktopDuplication
{
    internal class DesktopDuplicatorReader
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Logic.LogicManager _logic;
        private readonly GUI.TraySettings _settings;

        public DesktopDuplicatorReader(Logic.LogicManager logic, GUI.TraySettings settings)
        {
            _logic = logic;
            _settings = settings;
            
            RefreshCapturingState();

            _log.Info($"DesktopDuplicatorReader created.");
        }

        private bool IsRunning { get; set; }
        private CancellationTokenSource _cancellationTokenSource;


        private void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;

            switch (isRunning)
            {
                case true:
                {
                    _log.Debug("stopping the capturing");
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = null;
                    break;
                }
                case false:
                {
                    _log.Debug("starting the capturing");
                    _cancellationTokenSource = new CancellationTokenSource();
                    var thread = new Thread(() => Run(_cancellationTokenSource.Token))
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Normal,
                        Name = "DesktopDuplicatorReader"
                    };
                    thread.Start();
                    break;
                }
            }
        }
       
        private DesktopDuplicator _desktopDuplicator;

        private void Run(CancellationToken token)
        {
            if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            IsRunning = true;
            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            try
            {
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

                    const int minFrameTimeInMs = 1; //1000/FPS
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
                _desktopDuplicator = new DesktopDuplicator(0, _settings.SelectedMonitor);
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
