using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Framework.Animation
{
    public class AnimationGroup : MonoBehaviour
    {
        [SerializeField]
        private bool _enableLogging;

        public event Action<AnimationGroup> Completed;

        private List<AnimationBase> _animations;

        void Awake()
        {
            _animations = new List<AnimationBase>();

            foreach (var anim in GetComponents<AnimationBase>())
            {
                _animations.Add(anim);
                anim.Triggered += OnAnimationEvent;
            }
        }

        public void Play()
        {
            UpdateQueue();
        }

        public void Stop()
        {
            _animations.ForEach(x => x.Stop());
        }

        void UpdateQueue()
        {
            var playbackQueue = _animations.Where(x => x.State == AnimationPlaybackState.Stopped).ToList();

            foreach (var item in playbackQueue)
            {
                Log("Playing '" + item.GetType().Name + "'");
                item.Play();

                if (item.WaitForCompletion)
                {
                    Log("Waiting for '" + item.GetType().Name + "'");
                    break;
                }
            }
        }

        void OnAnimationEvent(AnimationEvent obj)
        {
            if(obj.PlaybackType == AnimationEventType.PlayComplete)
            {
                if (_animations.TrueForAll(x => x.State == AnimationPlaybackState.PlayComplete))
                {
                    Completed.InvokeSafe(this);
                }
                else
                {
                    UpdateQueue();
                }
            }
        }

        void Log(string message, params object[] args)
        {
            Debug.LogFormat("{0}: {1}", name, string.Format(message, args));
        }
    }
}
