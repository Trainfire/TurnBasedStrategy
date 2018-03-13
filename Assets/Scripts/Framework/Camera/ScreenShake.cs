using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace Framework.Components
{
    public class ScreenShake : ScreenEffect
    {
        public float Frequency;
        public float Amplitude;
        public float Duration;

        private float _startTime;
        private float _shakeTime;
        private float _lastTickTime;
        private float _shakeAmplitude;

        public Vector3 Offset { get; private set; }
        public bool Shaking { get; private set; }

        public override void Activate()
        {
            _startTime = Time.realtimeSinceStartup;
            _shakeTime = 0f;
            _shakeAmplitude = Amplitude;
            Shaking = true;
        }

        public override void Deactivate()
        {
            Shaking = false;
            Offset = Vector2.zero;
            OnFinish();
        }

        private void OnTick()
        {
            var rndValues = new float[2];
            for (int i = 0; i < rndValues.Length; i++)
            {
                var rnd = Random.value;
                int dir = rnd < 0.5f ? -1 : 1;
                rndValues[i] = dir * rnd * _shakeAmplitude;
            }

            Offset = new Vector3(rndValues[0], rndValues[1], 0f);
            transform.position += Offset;
        }

        public override void ProcessEffect()
        {
            if (Shaking)
            {
                _shakeTime += Time.deltaTime / Duration;

                if (Time.realtimeSinceStartup > _startTime + Duration)
                {
                    Deactivate();
                }
                else if (Time.realtimeSinceStartup > _lastTickTime + Frequency)
                {
                    _lastTickTime = Time.realtimeSinceStartup;
                    OnTick();
                }

                // Decay shake over time.
                _shakeAmplitude = Mathf.Lerp(Amplitude, 0f, _shakeTime);
            }
        }
    }
}
