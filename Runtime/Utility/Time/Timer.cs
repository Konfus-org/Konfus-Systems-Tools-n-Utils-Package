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
        public double TimeRemaining { get; private set; } = 0;

        public delegate void TimerEvent();

        /// <summary>
        ///     Event called every tick of timer.
        /// </summary>
        public event TimerEvent OnTimerTick;
        
        /// <summary>
        ///     Event called when the timer stops.
        /// </summary>
        public event TimerEvent OnTimerStop;

        private SynchronizationContext _syncContext;
        private System.Timers.Timer _systemTimer;
        private Stopwatch _stopwatch;
        private double _durationInMilliseconds;
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

            _systemTimer.Elapsed += OnTick;

            // Move operations to the main thread (required for certain Unity APIs).
            _syncContext = SynchronizationContext.Current;
            _syncContext.Send(state =>
            {
                if (onStop != null) OnTimerStop += onStop;
                if (onTick != null) OnTimerTick += onTick;
            }, null);
        }

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        public void Start(double durationInMilliseconds = -1, TimerEvent onTick = null, TimerEvent onStop = null)
        {
            _syncContext.Send(state =>
            {
                if (onStop != null) OnTimerStop += onStop;
                if (onTick != null) OnTimerTick += onTick;
            }, null);
            
            if (durationInMilliseconds == -1) durationInMilliseconds = Duration;
            if (durationInMilliseconds == 0) return;
            
            if (!IsPaused)
            {
                _durationInMilliseconds = durationInMilliseconds;
                _systemTimer.Interval = durationInMilliseconds;
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
            _systemTimer.Interval = TimeRemaining;
        }

        /// <summary>
        ///     Stops the timer.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            _currentTickCount = 0;
            _systemTimer.Interval = TimeRemaining = _durationInMilliseconds;
            _systemTimer.Stop();
            _stopwatch.Stop();
            _stopwatch.Reset();
        }

        /// <summary>
        ///     Destroys the timer.
        /// </summary>
        public void Destroy()
        {
            _systemTimer.Close();
            _systemTimer.Dispose();
            _systemTimer = null;
            _stopwatch = null;
            _syncContext = null;
            OnTimerStop = null;
            OnTimerTick = null;
        }

        private void OnTick(object source, ElapsedEventArgs elapsedEventArguments)
        {
            if (_currentTickCount < 1)
            {
                // Calls user-set method (Internal timer resets automatically).
                _syncContext.Send(state => { OnTimerTick?.Invoke(); }, null);
            }
            else
            {
                // Calls user-set method, also stops internal timer (which is normally set to repeat).
                _syncContext.Send(state => { OnTimerStop?.Invoke(); }, null);
                Stop();
            }
            
            TimeRemaining = Duration - _stopwatch.Elapsed.TotalMilliseconds;
            _currentTickCount++;
        }
    }
}