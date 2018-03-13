using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public abstract class Tween<T> : MonoBehaviour
    {
        public event Action<Tween<T>> OnDone;
        public UnityAction<T> OnTweenValue;
        public float Duration;
        public AnimationCurve Curve;
        public bool DoTween = false;

        public T Value { get; private set; }
        public T From { get; set; }
        public T To { get; set; }

        public bool Tweening { get; set; }
        float CurrentTime { get; set; }

        public void Play()
        {
            CurrentTime = 0f;
            DoTween = true;
        }

        public void Stop()
        {
            DoTween = false;
            Tweening = false;
        }

        protected abstract T OnTween(float delta);

        T DefaultCurve(T value)
        {
            return value;
        }

        void Update()
        {
            if (DoTween)
            {
                DoTween = false;
                Tweening = true;
                CurrentTime = 0f;
            }

            if (Tweening)
            {
                float t = CurrentTime / Duration;

                if (Curve != null)
                    t = Curve.Evaluate(t);

                Value = OnTween(t);

                if (OnTweenValue != null)
                    OnTweenValue(Value);

                CurrentTime += Time.deltaTime;

                if (CurrentTime > Duration)
                {
                    Value = To;

                    Tweening = false;

                    if (OnDone != null)
                        OnDone(this);
                }
            }
        }
    }

    public static class TweenHelper
    {
        public static TweenFloat Create(float from, float to, float duration)
        {
            var tween = new TweenFloat();
            tween.From = from;
            tween.To = to;
            tween.Duration = duration;
            return tween;
        }
    }
}
