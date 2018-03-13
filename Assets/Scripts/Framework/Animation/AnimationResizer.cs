using UnityEngine;

namespace Framework.Animation
{
    class AnimationResizer : AnimationBase
    {
        [SerializeField] private Vector2 _size;

        private RectTransform _rectTransform;
        private TweenVector2 _tweenVec;

        void Awake()
        {
            _rectTransform = Target.GetComponent<RectTransform>();

            _tweenVec = gameObject.AddComponent<TweenVector2>();
            _tweenVec.OnTweenValue += OnTween;
            _tweenVec.OnDone += OnTweenDone;
        }

        private void OnTweenDone(Tween<Vector2> obj)
        {
            TriggerPlayComplete();
        }

        protected override void OnPlay()
        {
            base.OnPlay();

            if (_rectTransform != null)
            {
                _tweenVec.Curve = Curve;
                _tweenVec.Duration = Duration;
                _tweenVec.From = _rectTransform.sizeDelta;
                _tweenVec.To = _size;
                _tweenVec.Play();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            _tweenVec.Stop();
        }

        void OnTween(Vector2 v)
        {
            _rectTransform.sizeDelta = v;
        }
    }
}
