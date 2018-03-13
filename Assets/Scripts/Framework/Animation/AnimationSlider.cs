using System;
using UnityEngine;

namespace Framework.Animation
{
    class AnimationSlider : AnimationBase
    {
        [SerializeField]
        private bool _relative;

        [SerializeField]
        private Vector3 _position;

        private BaseBehaviour _behaviour;
        private TweenVector3 _tweenVec;

        void Awake()
        {
            var rect = Target.GetComponent<RectTransform>();
            _behaviour = rect != null ? new RectTransformBehaviour(rect) as BaseBehaviour : new TransformBehaviour(Target.transform);

            _tweenVec = gameObject.AddComponent<TweenVector3>();
            _tweenVec.OnTweenValue += OnTween;
            _tweenVec.OnDone += OnTweenDone;
        }

        void OnTweenDone(Tween<Vector3> obj)
        {
            TriggerPlayComplete();
        }

        protected override void OnPlay()
        {
            base.OnPlay();

            if (_behaviour != null)
            {
                _tweenVec.Curve = Curve;
                _tweenVec.Duration = Duration;
                _tweenVec.From = _behaviour.Position;
                _tweenVec.To = _relative ? _behaviour.Position + _position : _position;
                _tweenVec.Play();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            _tweenVec.Stop();
        }

        void OnTween(Vector3 v)
        {
            _behaviour.Position = v;
        }
    }

    abstract class BaseBehaviour
    {
        public abstract Vector3 Position { get; set; }
    }

    class TransformBehaviour : BaseBehaviour
    {
        private Transform _transform;

        public TransformBehaviour(Transform transform)
        {
            _transform = transform;
        }

        public override Vector3 Position
        {
            get { return _transform.position; }
            set { _transform.position = value; }
        }
    }

    class RectTransformBehaviour : BaseBehaviour
    {
        private RectTransform _transform;

        public RectTransformBehaviour(RectTransform transform)
        {
            _transform = transform;
        }

        public override Vector3 Position
        {
            get { return _transform.anchoredPosition; }
            set { _transform.anchoredPosition3D = value; }
        }
    }
}
