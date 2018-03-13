using UnityEngine;

namespace Framework.Animation
{
    class AnimationScaler : AnimationBase
    {
        [SerializeField] private bool _useFromValue;
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        
        private TweenVector3 _tweenVec;

        void Awake()
        {
            _tweenVec = gameObject.AddComponent<TweenVector3>();
            _tweenVec.OnDone += OnTweenDone;
            _tweenVec.OnTweenValue += OnTween;
        }

        protected override void OnPlay()
        {
            base.OnPlay();

            _tweenVec.Curve = Curve;
            _tweenVec.Duration = Duration;
            _tweenVec.From = _useFromValue ? _from : Target.transform.localScale;
            _tweenVec.To = _to;
            _tweenVec.Play();
        }

        private void OnTweenDone(Tween<Vector3> obj)
        {
            Target.transform.localScale = obj.Value;
            TriggerPlayComplete();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _tweenVec.Stop();
        }

        void OnTween(Vector3 scale)
        {
            Target.transform.localScale = scale;
        }
    }
}
