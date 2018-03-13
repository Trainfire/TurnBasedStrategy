using System;
using UnityEngine;

namespace Framework
{
    public static class Services
    {
        private static IAudioSystem _audio;
        public static IAudioSystem Audio { get { return _audio; } }

        static Services()
        {
            _audio = new NullAudioSystem();
        }

        public static void Provide(IAudioSystem audioSystem)
        {
            _audio = audioSystem == null ? new NullAudioSystem() : audioSystem;
        }
    }

    public interface IAudioSystem
    {
        void PlayMusic(AudioClip audioClip, bool isLooping = false);
        AudioSource PlaySound(AudioClip audioClip, float volume, bool isLooping = false);
        AudioSource PlaySoundPositional(AudioClip audioClip, float volume, GameObject target, bool isLooping = false);
        AudioSource PlaySoundPositional(AudioClip audioClip, float volume, Vector3 position, bool isLooping = false);
        void StopSound(AudioSource audioClip);
        void StopAllSounds();
    }

    public class NullAudioSystem : IAudioSystem
    {
        void IAudioSystem.PlayMusic(AudioClip audioClip, bool isLooping) { }
        AudioSource IAudioSystem.PlaySound(AudioClip audioClip, float volume, bool isLooping) { return null; }
        AudioSource IAudioSystem.PlaySoundPositional(AudioClip audioClip, float volume, GameObject target, bool isLooping) { return null; }
        AudioSource IAudioSystem.PlaySoundPositional(AudioClip audioClip, float volume, Vector3 position, bool isLooping) { return null; }
        void IAudioSystem.StopAllSounds() { }
        void IAudioSystem.StopSound(AudioSource audioClip) { }
    }
}
