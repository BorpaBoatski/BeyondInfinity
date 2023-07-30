using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class SFX
{
    public string Name;
    public AudioClip AudioClip;
    public bool IsLooping;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public float DefaultMusicVolume = 0.3f;
    public float DefaultSFXVolume = 0.3f;

    public SFX[] SFXClips;
    public AudioSource MusicSource;
    public List<AudioSource> SFXSource = new List<AudioSource>();

    public GameObject SFXSourcePrefab;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        MusicSource.volume = DefaultMusicVolume;
        MusicSource.Play();
    }

    public void PlaySFX(string name)
    {
        foreach(SFX s in SFXClips)
        {
            if (s.Name == name)
            {
                AudioSource _targetSource = FindSFXSource(s.AudioClip);
                if (_targetSource != null)
                {
                    if (!_targetSource.isPlaying)
                    {
                        _targetSource.Play();
                    }
                    return;
                }

                AudioSource newSource = Instantiate(SFXSourcePrefab, transform).GetComponent<AudioSource>();
                newSource.clip = s.AudioClip;
                newSource.loop = s.IsLooping;
                newSource.volume = DefaultSFXVolume;  
                newSource.Play();
                SFXSource.Add(newSource);
            }
        }
    }

    public void StopSFX(string name)
    {
        foreach (SFX s in SFXClips)
        {
            if (s.Name == name)
            {
                AudioSource _targetSource = FindSFXSource(s.AudioClip);
                if (_targetSource == null) return;

                _targetSource.Stop();
            }
        }
    }

    AudioSource FindSFXSource(AudioClip clip)
    {
        for (int i = 0; i < SFXSource.Count; i++)
        {
            if (SFXSource[i].clip == clip)
            {
                return SFXSource[i];
            }
        }
        return null;
    }

    public void UpdateMusicVolume(float value)
    {
        MusicSource.volume = DefaultMusicVolume * value;
    }

    public void UpdateSFXVolume(float value)
    {
        foreach(AudioSource sfx in SFXSource) 
        { 
            sfx.volume = DefaultSFXVolume * value;
        }
    }
}
