using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace Konfus.Utility.Time
{
    /// <summary>
    /// A reusable System.Timers based timer for Unity.
    /// </summary>
    public class Timer
    {
        private const int Tickrate = 30;
        private readonly Stopwatch _stopwatch;
        private readonly SystemTimer? _systemTimer;
        private int _currentTickCount;

        private Action? _onTimerStart;
        private Action? _onTimerStop;
        private Action? _onTimerTick;
        private SynchronizationContext? _syncContext;

        public Timer(double durationInMilliseconds, Action? onStart = null,
            Action? onTick = null, Action? onStop = null)
        {
            Duration = durationInMilliseconds;
            _onTimerStart = onStart;
            _onTimerTick = onTick;
            _onTimerStop = onStop;

            _systemTimer = new SystemTimer();
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Whether the timer is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Whether a timer is paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// The time remaining on the timer in milliseconds.
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// The time remaining on the timer in milliseconds.
        /// </summary>
        public double TimeRemaining => Duration - _stopwatch.ElapsedMilliseconds;

        public void SetOnStartAction(Action action)
        {
            _onTimerStart = action;
        }

        public void SetOnTickAction(Action action)
        {
            _onTimerTick = action;
        }

        public void SetOnStopAction(Action action)
        {
            _onTimerStop = action;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            Stop();

            if (_systemTimer == null) return;
            _syncContext = SynchronizationContext.Current;
            _systemTimer.Interval = Tickrate;
            _systemTimer.Elapsed += OnTimerElapsed;

            IsRunning = true;
            IsPaused = false;
            _currentTickCount = 0;
            _systemTimer.Start();
            _stopwatch.Start();

            Action? onTimerStartAction = _onTimerStart;

            _syncContext?.Send(_ =>
            {
                // Send start synchronously
                onTimerStartAction?.Invoke();
            }, null);
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        public void Pause()
        {
            // Stop things.
            IsRunning = false;
            IsPaused = true;
            _systemTimer?.Stop();
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

            _stopwatch.Stop();
            IsRunning = false;
            IsPaused = false;
            _currentTickCount = 0;

            _syncContext?.Send(_ =>
            {
                // Send stop synchronously
                _onTimerStop?.Invoke();
            }, null);

            //not resetting _onTimerStop to null because that prevents the async invoke from calling stop action
            //not resetting _OnTimerTick to null. Prevents OnElapsed from invoking Action 
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs elapsedEventArguments)
        {
            if (_currentTickCount * Tickrate <= Duration)
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