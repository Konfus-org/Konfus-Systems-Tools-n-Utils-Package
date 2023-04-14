using System;
using System.Collections;
using UnityEngine;

namespace Konfus.Utility.Time
{
    /// <summary>
    ///     <para> A reusable Coroutine based timer. </para>
    /// </summary>
    public class CoTimer
    {

        private float _durationInSeconds;
        private readonly Action _onTick;
        private readonly Action _onStop;
        private readonly MonoBehaviour _owner;

        /// <summary>
        ///     How much time is remaining on a timer.
        /// </summary>
        public float TimeRemaining { get; private set; }

        /// <summary>
        ///     Whether or not a timer is running.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        ///     Whether or not a timer is paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        private bool _pause;
        private Coroutine _timer;
        
        /// <summary>
        ///     <para> Creates a reusable Coroutine based timer. </para>
        ///     <param name="owner"> The MonoBehaviour that owns the timer and calls its coroutine. </param>
        ///     <param name="onTick"> Calls this action every tick of the timer. </param>
        ///     <param name="onStop"> Calls this action on timer end. </param>
        /// </summary>
        public CoTimer(MonoBehaviour owner, Action onTick = null, Action onStop = null)
        {
            _owner = owner;
            _onTick = onTick;
            _onStop = onStop;
        }

        /// <summary>
        ///     Stops the timer.
        /// </summary>
        public void Stop()
        {
            _owner.StopCoroutine(_timer);

            TimeRemaining = 0;
            _pause = false;
            IsRunning = false;
            IsPaused = false;
            _timer = null;
        }

        /// <summary>
        ///     Pauses the timer.
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
            _pause = true;
        }

        /// <summary>
        ///     <para> Starts the timer that runs for the specified amount of seconds. </para>
        ///     <param name="durationInSeconds"> How many seconds the timer will run. </param>
        /// </summary>
        public void Start(float durationInSeconds)
        {
            _durationInSeconds = durationInSeconds;
            _timer = _owner.StartCoroutine(StartTimerCoroutine());
        }

        private IEnumerator StartTimerCoroutine()
        {
            IsRunning = true;
            float timeElapsed = 0;
            while (timeElapsed <= _durationInSeconds)
            {
                if (_pause)
                {
                    _durationInSeconds = TimeRemaining;
                    yield return null;
                }

                _onTick?.Invoke();
                timeElapsed += UnityEngine.Time.deltaTime;
                TimeRemaining = _durationInSeconds - timeElapsed;

                yield return null;
            }

            _timer = null;
            TimeRemaining = 0;
            _pause = false;
            IsRunning = false;
            IsPaused = false;
            _timer = null;
            
            _onStop?.Invoke();
        }
    }
}