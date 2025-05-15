using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioPair[] audioNamesAndClips;
    public static AudioPlayer instance;

    private Dictionary<string, AudioClip> soundDict = new Dictionary<string, AudioClip>();


    [Serializable]
    public struct AudioPair
    {
        public string name;
        public AudioClip clip;
    }

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        foreach (AudioPair pair in audioNamesAndClips)
        {
            soundDict.Add(pair.name, pair.clip);
        }
    }


    public void PlaySound(string name)
    {
        AudioClip clip;
        soundDict.TryGetValue(name, out clip);

        if (clip == null)
        {
            Debug.LogError($"Sound {name} doesn't exist.");
            return;
        }
        audioSource.PlayOneShot(clip);
    }
}
