using System;
using UnityEngine;

namespace Framework.Animation
{
    class AnimationFader : AnimationBase
    {
        [SerializeField] private FadeType _fadeType;
        private CanvasGroup _canvasGroup;
        private TweenFloat _tweenFloat;

        void Awake()
        {
            _canvasGroup = Target.GetOrAddComponent<CanvasGroup>();

            _tweenFloat = gameObject.AddComponent<TweenFloat>();
            _tweenFloat.Duration = Duration;
            _tweenFloat.OnDone += OnTweenDone;
            _tweenFloat.OnTweenValue += OnTween;
        }

        protected override void OnPlay()
        {
            base.OnPlay();

            _tweenFloat.From = _canvasGroup.alpha;
            _tweenFloat.To = _fadeType == FadeType.In ? 1f : 0f;
            _tweenFloat.Play();
        }

        private void OnTweenDone(Tween<float> obj)
        {
            _canvasGroup.alpha = _tweenFloat.To;
            TriggerPlayComplete();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _tweenFloat.Stop();
        }

        void OnTween(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }
    }
}
