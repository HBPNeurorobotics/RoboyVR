namespace Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] AudioSource[] _hoverSounds;

        [HideInInspector] public AudioSource _currentAudio;

        void startAudio(AudioSource source)
        {
            if (source.isPlaying)
                return;
            source.Play();
        }

        public void setCurrentAudio(AudioClip clip)
        {
            _currentAudio.Stop();
            _currentAudio.clip = clip;
        }

        public void startHovering()
        {
            foreach (AudioSource audio in _hoverSounds)
            {
                startAudio(audio);
            }
        }

        public void stopHovering()
        {
            foreach (AudioSource audio in _hoverSounds)
            {
                audio.Stop();
            }
        }
    } 
}
