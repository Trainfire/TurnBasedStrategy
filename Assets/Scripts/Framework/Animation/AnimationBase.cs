using UnityEngine;
using System;

namespace Framework.Animation
{
    public enum AnimationPlaybackState
    {
        Stopped,
        Playing,
        PlayComplete,
    }

    public enum AnimationEventType
    {
        None,
        Playing,
        Stopping,
        PlayComplete,
    }

    public class AnimationEvent : EventBase<AnimationBase>
    {
        public AnimationEventType PlaybackType { get; private set; }

        public AnimationEvent(AnimationBase sender, AnimationEventType playbackType) : base(sender)
        {
            PlaybackType = playbackType;
        }
    }

    public abstract class AnimationBase : MonoBehaviour
    {
        public event Action<AnimationEvent> Triggered;

        [SerializeField] private GameObject _target;
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private bool _waitForCompletion;

        protected GameObject Target { get { return _target; } }
        protected float Duration { get { return _duration; } }
        protected AnimationCurve Curve { get { return _curve; } }
        public AnimationPlaybackState State { get; private set; }
        public bool WaitForCompletion { get { return _waitForCompletion; } }

        public void Play()
        {
            State = AnimationPlaybackState.Playing;
            Triggered.InvokeSafe(new AnimationEvent(this, AnimationEventType.Playing));
            OnPlay();
        }

        public void Stop()
        {
            State = AnimationPlaybackState.Stopped;
            Triggered.InvokeSafe(new AnimationEvent(this, AnimationEventType.Stopping));
            OnStop();
        }

        protected virtual void OnPlay() { }
        protected virtual void OnStop() { }

        protected void TriggerPlayComplete()
        {
            State = AnimationPlaybackState.PlayComplete;
            Triggered.InvokeSafe(new AnimationEvent(this, AnimationEventType.PlayComplete));
        }
    }
}
