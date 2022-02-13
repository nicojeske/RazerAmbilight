using MoreLinq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Ambilight.Util
{
    /// <summary>
    /// Class for logging the real FPS
    /// </summary>
    internal sealed class FpsLogger : IDisposable
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly string _name;
        private readonly Subject<Unit> _frames = new Subject<Unit>();

#if DEBUG
        //infinite timespan sequence: forever 1s
        private readonly IEnumerable<TimeSpan> _logTimes = MoreEnumerable.Generate(TimeSpan.FromSeconds(1), _ => TimeSpan.FromSeconds(1));
#else
        //infinite timespan sequence: 10x 1s, then forever 5min
        private readonly IEnumerable<TimeSpan> _logTimes =
            Enumerable.Repeat(TimeSpan.FromSeconds(1), 10)
            .Concat(MoreEnumerable.Generate(TimeSpan.FromMinutes(5), _ => TimeSpan.FromMinutes(5)));
#endif

        private readonly IEnumerator<TimeSpan> _enumerator;
        private readonly IDisposable _loggingSubscription;
        private readonly IDisposable _valueUpdatingSubscription;


        public FpsLogger(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _enumerator = _logTimes.GetEnumerator();

            var loggingTrigger = Observable.Generate(1, _ => true, i => i + 1, i => i, _ =>
            {
                _enumerator.MoveNext();
                return _enumerator.Current;
            });

            var fpsObserverable = _frames
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(nums => nums.Count);

            _valueUpdatingSubscription = fpsObserverable.Subscribe(f => { });

            _loggingSubscription = loggingTrigger
                .WithLatestFrom(fpsObserverable, (_, fps) => fps)
                .Subscribe(WriteFpsLog);
        }

        /// <summary>
        /// Writes the current fps to the log
        /// </summary>
        /// <param name="fps"></param>
        private void WriteFpsLog(int fps)
        {
            _log.Debug($"there were {fps} frames for {_name} in the last second.");
        }

        /// <summary>
        /// Tracks a single frame
        /// </summary>
        public void TrackSingleFrame() => _frames.OnNext(Unit.Default);

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            _valueUpdatingSubscription?.Dispose();
            _loggingSubscription?.Dispose();
            _frames?.Dispose();
            _enumerator?.Dispose();
        }
    }
}
