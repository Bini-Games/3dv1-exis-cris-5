﻿using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class SoundPlayer : MonoBehaviour
    {
        private List<AudioSource> audioSources = new List<AudioSource>();

        private static HashSet<AudioClip> loopedClips = new HashSet<AudioClip>();

        public static SoundPlayer Instance;

        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.DontSave;
        }

        private void Update()
        {
            for (var index = audioSources.Count - 1; index >= 0; index--)
            {
                var audioSource = audioSources[index];

                if (!audioSource || !audioSource.isPlaying)
                {
                    Destroy(audioSource.gameObject);
                    audioSources.RemoveAt(index);
                }
            }
        }

        public bool IsPlaying(AudioClip clip)
        {
            if (loopedClips.Contains(clip))
                return true;

            foreach (var audioSource in audioSources)
            {
                if (audioSource && (audioSource.clip == clip) && audioSource.isPlaying)
                    return true;
            }

            return false;
        }

        public void Play(AudioClip clip, float volume = 1, bool loop = false)
        {
            var soundObject = new GameObject(clip.name);
            soundObject.transform.SetParent(transform, false);

            var audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.loop = loop;
            audioSource.playOnAwake = false;
            audioSource.spatialize = false;
            audioSource.PlayOneShot(clip, volume);

            audioSources.Add(audioSource);

            if (loop)
                loopedClips.Add(clip);
        }
    }
}