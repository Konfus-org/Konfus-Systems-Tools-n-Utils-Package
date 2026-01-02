using UnityEngine;

namespace Konfus.Oscilation
{
    public class Oscillator : MonoBehaviour
    {
        public enum OscillationMode
        {
            Position,
            Rotation,
            Scale
        }

        [Header("Oscillation Settings")]
        [SerializeField]
        private OscillationMode mode = OscillationMode.Position;

        [SerializeField]
        private Vector3 amplitude = new(0f, 1f, 0f);
        [SerializeField]
        private float frequency = 1f;

        [Header("Time Settings")]
        [SerializeField]
        private bool useUnscaledTime;

        private Vector3 _startValue;

        private void Awake()
        {
            _startValue = mode switch
            {
                OscillationMode.Position => transform.localPosition,
                OscillationMode.Rotation => transform.localEulerAngles,
                OscillationMode.Scale => transform.localScale,
                _ => Vector3.zero
            };
        }

        private void Update()
        {
            float time = useUnscaledTime ? Time.unscaledTime : Time.time;
            float wave = Mathf.Sin(time * frequency * Mathf.PI * 2f);

            Vector3 offset = amplitude * wave;
            Vector3 value = _startValue + offset;

            switch (mode)
            {
                case OscillationMode.Position:
                    transform.localPosition = value;
                    break;

                case OscillationMode.Rotation:
                    transform.localEulerAngles = value;
                    break;

                case OscillationMode.Scale:
                    transform.localScale = value;
                    break;
            }
        }
    }
}