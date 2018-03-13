using UnityEngine;
using System;

namespace Framework
{
    /// <summary>
    /// Listens to an AudioSource and calls back when it has finished playing.
    /// </summary>
    public class AudioSourceListener : MonoBehaviour
    {
        public event Action<AudioSourceListener> FinishedPlaying;
        public event Action<AudioSourceListener> Destroyed;

        public AudioSource AudioSource { get; private set; }

        private bool _startedPlaying;

        public void SetAudioSource(AudioSource source)
        {
            AudioSource = source;
        }

        public void Update()
        {
            if (AudioSource == null)
                return;

            if (AudioSource.isPlaying && !_startedPlaying)
                _startedPlaying = true;

            if (_startedPlaying && !AudioSource.isPlaying && !AudioSource.loop)
            {
                FinishedPlaying.InvokeSafe(this);
                _startedPlaying = false;
            }
        }

        public void OnDestroy()
        {
            Destroyed.InvokeSafe(this);
        }
    }
}
