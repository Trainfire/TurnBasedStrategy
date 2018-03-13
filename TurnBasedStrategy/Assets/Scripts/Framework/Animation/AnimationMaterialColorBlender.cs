using System;
using UnityEngine;

namespace Framework.Animation
{
    class AnimationMaterialColorBlender : AnimationBase
    {
        public Material _material;

        [SerializeField] private bool _useSharedMaterial;
        [SerializeField] private Color _from;
        [SerializeField] private Color _to;

        //private Renderer _material;
        private TweenColor _tween;

        void Awake()
        {
            _tween = gameObject.AddComponent<TweenColor>();
            _tween.Duration = Duration;
            _tween.OnDone += OnTweenDone;
            _tween.OnTweenValue += OnTween;
        }

        protected override void OnPlay()
        {
            base.OnPlay();

            _tween.From = _from;
            _tween.To = _to;
            _tween.Play();
        }

        private void OnTweenDone(Tween<Color> obj)
        {
            Material.color = obj.Value;
            TriggerPlayComplete();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _tween.Stop();
        }

        void OnTween(Color color)
        {
            Material.color = color;
        }

        Material Material
        {
            get
            {
                if (_material == null)
                {
                    if (_useSharedMaterial)
                    {
                        _material = Target.GetComponent<Renderer>().sharedMaterial;
                    }
                    else
                    {
                        _material = Target.GetComponent<Renderer>().material;
                    }
                }
                return _material;
            }
        }
    }
}
