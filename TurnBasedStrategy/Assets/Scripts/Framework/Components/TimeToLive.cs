using UnityEngine;
using System;

namespace Framework
{
    class TimeToLive : MonoBehaviour
    {
        public event Action<TimeToLive> Destroyed;

        [SerializeField] private bool _startAutomatically;
        [SerializeField] private float _delay;

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        private float _timeStamp;
        private bool _started;

        void Start()
        {
            if (_startAutomatically)
                StartTimer();
        }

        public void StartTimer()
        {
            _started = true;
            _timeStamp = Time.timeSinceLevelLoad;
        }

        public void Update()
        {
            if (!_started)
                return;

            if (Time.time > _timeStamp + Delay)
            {
                Destroyed.InvokeSafe(this);
                Destroy(gameObject);
            }
        }
    }
}
