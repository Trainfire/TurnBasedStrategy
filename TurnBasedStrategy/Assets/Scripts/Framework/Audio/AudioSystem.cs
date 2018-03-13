using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Framework
{
    public class AudioSystem : MonoBehaviour, IAudioSystem
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioSource _musicSource;

        private List<AudioSource> _liveSounds;

        private void Awake()
        {
            _liveSounds = new List<AudioSource>();
        }

        void IAudioSystem.PlayMusic(AudioClip audioClip, bool isLooping)
        {
            _musicSource.Stop();
            _musicSource.clip = audioClip;
            _musicSource.loop = isLooping;
            _musicSource.Play();
        }

        AudioSource IAudioSystem.PlaySound(AudioClip clip, float volume, bool isLooping)
        {
            var source = CreateSoundSource(gameObject);
            PlaySound(source, clip, volume, isLooping);
            return source;
        }

        AudioSource IAudioSystem.PlaySoundPositional(AudioClip clip, float volume, GameObject target, bool isLooping)
        {
            var source = CreateSoundSource(target);
            source.spatialBlend = 1f;
            PlaySound(source, clip, volume, isLooping);
            return source;
        }

        AudioSource IAudioSystem.PlaySoundPositional(AudioClip clip, float volume, Vector3 position, bool isLooping)
        {
            var source = CreateSoundSource(position);
            source.spatialBlend = 1f;
            PlaySound(source, clip, volume, isLooping);
            return source;
        }

        void IAudioSystem.StopAllSounds()
        {
            _liveSounds.ToList().ForEach(s =>
            {
                s.Stop();
                UnregisterSoundSource(s);
            });
        }

        void IAudioSystem.StopSound(AudioSource source)
        {
            if (_liveSounds.Contains(source))
            {
                UnregisterSoundSource(source);
            }
            else
            {
                Debug.LogWarning("[AudioSystem] Cannot stop sound as it does not exist.");
            }
        }

        private void PlaySound(AudioSource source, AudioClip clip, float volume, bool isLooping)
        {
            var sfxGroup = _mixer.FindMatchingGroups("SFX");
            source.outputAudioMixerGroup = sfxGroup[0];
            source.clip = clip;
            source.volume = volume;
            source.loop = isLooping;
            source.Play();
        }

        private AudioSource CreateSoundSource(Vector3 position)
        {
            var sound = new GameObject().AddComponent<AudioSource>();
            sound.transform.position = position;
            RegisterSoundSource(sound);
            return sound;
        }

        private AudioSource CreateSoundSource(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("[AudioSystem] The specified target is null.");
                return null;
            }
            else
            {
                var sound = target.AddComponent<AudioSource>();
                RegisterSoundSource(sound);
                return sound;
            }
        }

        private void RegisterSoundSource(AudioSource source)
        {
            // Listen to the audio source so we can tell when it's finished playing and when it's destroyed.
            var listener = source.gameObject.AddComponent<AudioSourceListener>();
            listener.FinishedPlaying += Listener_Event;
            listener.Destroyed += Listener_Event;
            listener.SetAudioSource(source);

            _liveSounds.Add(source);
        }

        private void Listener_Event(AudioSourceListener listener)
        {
            listener.FinishedPlaying -= Listener_Event;
            listener.Destroyed -= Listener_Event;

            Destroy(listener);

            UnregisterSoundSource(listener.AudioSource);
        }

        private void UnregisterSoundSource(AudioSource source)
        {
            if (_liveSounds.Contains(source))
            {
                source.Stop();
                Destroy(source);
                _liveSounds.Remove(source);
            }
            else
            {
                Debug.LogError("[AudioSystem] The specified audio source has not been registered.");
            }
        }
    }
}
