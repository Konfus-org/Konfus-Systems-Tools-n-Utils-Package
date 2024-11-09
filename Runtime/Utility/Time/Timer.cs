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
        
        private Action _onTimerStart;
        private Action _onTimerTick;
        private Action _onTimerStop;
        private SynchronizationContext _syncContext;
        private System.Timers.Timer _systemTimer;
        private Stopwatch _stopwatch;
        private int _currentTickCount;
        
        public Timer(double durationInMilliseconds, Action onStart = null,
            Action onTick = null, Action onStop = null)
            {
                Duration = durationInMilliseconds;
                _onTimerStart = onStart;
                _onTimerTick = onTick;
                _onTimerStop = onStop;
                
                _systemTimer = new System.Timers.Timer();
                _stopwatch = new Stopwatch();
            }
            
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
            
            _syncContext = SynchronizationContext.Current;

            _systemTimer.Interval = TICKRATE;
            _systemTimer.Elapsed += OnTimerElapsed;
            
            IsRunning = true;
            IsPaused = false;
            _currentTickCount = 0;
            _systemTimer.Start();
            _stopwatch.Start();
            
            var onTimerStartAction = _onTimerStart;
            
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