using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace Konfus.Utility.Time
{
    /// <summary>
    /// A reusable System.Timers based timer for Unity.
    /// </summary>
    public class Timer
    {
        private const int TICKRATE = 30;
        
        /// <summary>
        /// Whether or not the timer is running.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Whether or not a timer is paused.
        /// </summary>
        public bool IsPaused { get; private set; } = false;

        /// <summary>
        ///  The time remaining on the timer in milliseconds.
        /// </summary>
        public double Duration { get; private set; } = 0;

        /// <summary>
        /// The time remaining on the timer in milliseconds.
        /// </summary>
        public double TimeRemaining => Duration - _stopwatch.ElapsedMilliseconds;
        
        private Action _onTimerTick;
        private Action _onTimerStop;
        private SynchronizationContext _syncContext;
        private System.Timers.Timer _systemTimer;
        private Stopwatch _stopwatch;
        private int _currentTickCount;

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start(double durationInMilliseconds = -1, Action onTick = null, Action onStop = null)
        {
            Stop();
            
            _syncContext = SynchronizationContext.Current;
            _systemTimer = new System.Timers.Timer();
            _stopwatch = new Stopwatch();

            _systemTimer.Interval = TICKRATE;
            _systemTimer.Elapsed += OnTimerElapsed;
            
            if (onStop != null) _onTimerStop = onStop;
            if (onTick != null) _onTimerTick = onTick;
            
            if (durationInMilliseconds == -1) durationInMilliseconds = Duration;
            if (durationInMilliseconds == 0) return;
            if (!IsPaused) Duration = durationInMilliseconds;
            
            IsRunning = true;
            IsPaused = false;
            _currentTickCount = 0;
            _systemTimer.Start();
            _stopwatch.Start();
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        public void Pause()
        {
            // Stop things.
            IsRunning = false;
            IsPaused = true;
            _systemTimer.Stop();
            _stopwatch.Stop();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            if (_systemTimer != null)
            {
                _systemTimer.Elapsed -= OnTimerElapsed;
                _systemTimer.Stop();
                _systemTimer.Close();
            }

            _stopwatch?.Stop();
            
            IsRunning = false;
            IsPaused = false;
            _currentTickCount = 0;
            
            var onTimerStopAction = _onTimerStop;
            _onTimerStop = null;
            _onTimerTick = null;
            
            _syncContext?.Send(_ =>
            {
                // Send stop synchronously
                onTimerStopAction?.Invoke();
            }, null);
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs elapsedEventArguments)
        {
            if (_currentTickCount * TICKRATE <= Duration)
            {
                // Calls user-set method (Internal timer resets automatically).
                _syncContext?.Send(_ =>
                {
                    // Send tick synchronously
                    _onTimerTick?.Invoke();
                }, null);
            }
            else
            {
                // Stops internal timer (which is normally set to repeat).
                Stop();
            }
            
            _currentTickCount++;
        }
    }
}