using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace Konfus.Utility.Time
{
    /// <summary>
    /// A reusable System.Timers based timer for Unity.
    /// </summary>
    public class Timer : IDisposable
    {
        public const int TICKRATE = 100;
        
        /// <summary>
        ///     Whether or not the timer is running.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        ///     Whether or not a timer is paused.
        /// </summary>
        public bool IsPaused { get; private set; } = false;

        /// <summary>
        ///     The time remaining on the timer in milliseconds.
        /// </summary>
        public double Duration { get; private set; } = 0;

        /// <summary>
        ///     The time remaining on the timer in milliseconds.
        /// </summary>
        public double TimeRemaining => Duration - _stopwatch.ElapsedMilliseconds;

        public delegate void TimerEvent();

        /// <summary>
        ///     Event called every tick of timer.
        /// </summary>
        public event TimerEvent TimerTick;
        
        /// <summary>
        ///     Event called when the timer stops.
        /// </summary>
        public event TimerEvent TimerStopped;
        
        private TimerEvent _onTimerTick;
        private TimerEvent _onTimerStop;
        private SynchronizationContext _syncContext;
        private System.Timers.Timer _systemTimer;
        private Stopwatch _stopwatch;
        private int _currentTickCount;

        /// <summary>
        ///     <para> Creates a System.Timers based timer for Unity </para>
        ///     <param name="onTick"> The action to execute every tick. </param>
        ///     <param name="onStop"> The action to execute on timer stopped. </param>
        /// </summary>
        public Timer(double durationInMilliseconds = 0.01f, TimerEvent onTick = null, TimerEvent onStop = null)
        {
            Duration = durationInMilliseconds;
            _systemTimer = new System.Timers.Timer();
            _stopwatch = new Stopwatch();

            _systemTimer.Interval = TICKRATE;
            _systemTimer.Elapsed += OnTimerElapsed;
            TimerTick += OnTimerTick;
            TimerStopped += OnTimerStopped;

            // Move operations to the main thread (required for certain Unity APIs).
            _onTimerTick = onTick;
            _onTimerStop = onStop;
        }

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        public void Start(double durationInMilliseconds = -1, TimerEvent onTick = null, TimerEvent onStop = null)
        {
            _syncContext = SynchronizationContext.Current;
            
            if (onStop != null)
            {
                _onTimerStop = onStop;
            }
            
            if (onTick != null)
            {
                _onTimerTick = onTick;
            }
            
            if (durationInMilliseconds == -1) durationInMilliseconds = Duration;
            if (durationInMilliseconds == 0) return;
            
            if (!IsPaused)
            {
                Duration = durationInMilliseconds;
            }
            
            IsRunning = true;
            IsPaused = false;
            _currentTickCount = 0;
            _systemTimer.Start();
            _stopwatch.Start();
        }

        /// <summary>
        ///     Pauses the timer.
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
        ///     Stops the timer.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            _currentTickCount = 0;
            _systemTimer.Stop();
            _stopwatch.Stop();
            _stopwatch.Reset();
        }

        /// <summary>
        ///     Destroys the timer.
        /// </summary>
        public void Dispose()
        {
            TimerTick -= OnTimerTick;
            TimerStopped -= OnTimerStopped;
            _systemTimer.Elapsed -= OnTimerElapsed;
            
            _systemTimer.Close();
            _systemTimer.Dispose();
            _stopwatch.Stop();
            
            TimerStopped = null;
            TimerTick = null;
            
            _systemTimer = null;
            _stopwatch = null;
            _onTimerTick = null;
            _onTimerStop = null;
        }

        private void OnTimerTick()
        {
            _syncContext.Send(_ => _onTimerTick.Invoke(), null);
        }

        private void OnTimerStopped()
        {
            _syncContext.Send(_ => _onTimerStop.Invoke(), null);
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs elapsedEventArguments)
        {
            if (_currentTickCount * TICKRATE <= Duration)
            {
                // Calls user-set method (Internal timer resets automatically).
                TimerTick?.Invoke();
            }
            else
            {
                // Calls user-set method, also stops internal timer (which is normally set to repeat).
                TimerStopped?.Invoke();
                Stop();
            }
            
            _currentTickCount++;
        }
    }
}