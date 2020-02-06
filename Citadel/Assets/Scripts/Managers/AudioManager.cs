using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Variables & Inspector Options")]
    public AudioSource musicSource;
    public GameObject sfxPlayerPrefab;

    [Header("Music Settings")]
    [Space(10)]
    public List<MusicTrack> musicTracks;

    [HideInInspector]
    public GameVars gameVars;
    [HideInInspector]
    public List<AudioSource> sfxPlayers;

    private MusicTrack currentMusicTrack;
    private bool isLerpingFade;

    // Start is called before the first frame update
    void Start()
    {
        sfxPlayers = new List<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SlidersUI.instance && !isLerpingFade)
        {
            if (currentMusicTrack != null)
            {
                musicSource.volume = currentMusicTrack.musicRelativeVolume * gameVars.musicVolumeScale;
            }
            else
            {
                musicSource.volume = gameVars.musicVolumeScale;
            }
        }

        if (sfxPlayers != null && sfxPlayers.Count > 0)
        {
            for (int i = sfxPlayers.Count - 1; i > -1; i--)
            {
                if (sfxPlayers[i])
                {
                    if (sfxPlayers[i].isPlaying == false)
                    {
                        Destroy(sfxPlayers[i].gameObject);
                        sfxPlayers.RemoveAt(i);
                    }
                }
                else
                {
                    sfxPlayers.RemoveAt(i);
                }
            }
        }
    }

    public void PlayMusic(string musicKeyName)
    {
        foreach (MusicTrack track in musicTracks)
        {
            if (track.musicIdentifier == musicKeyName)
            {
                musicSource.Stop();
                currentMusicTrack = track;

                musicSource.loop = true;
                float totalVolume = currentMusicTrack.musicRelativeVolume * gameVars.musicVolumeScale;
                musicSource.volume = 0;
                musicSource.clip = currentMusicTrack.musicClip;
                musicSource.Play();
                StartCoroutine(FadeInAudioTrack(musicSource, totalVolume, 8.0f));
                break;
            }
        }
    }

    public IEnumerator FadeInAudioTrack(AudioSource audioSource, float totalVolume, float lerpDuration)
    {
        isLerpingFade = true;
        float t = 0;

        while (audioSource.volume != totalVolume)
        {
            t += Time.deltaTime / lerpDuration;
            audioSource.volume = Mathf.Lerp(audioSource.volume, totalVolume, t);
            yield return null;
        }
        isLerpingFade = false;
    }

    public IEnumerator FadeOutAudioTrack(AudioSource audioSource, float lerpDuration)
    {
        isLerpingFade = true;
        float defaultVolume = audioSource.volume;
        float t = 0;
        float currentVolume = audioSource.volume;

        while (currentVolume != 0)
        {
            if (audioSource != null)
            {
                t += Time.deltaTime / lerpDuration;
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0, t);
                currentVolume = audioSource.volume;
            }
            else
            {
                currentVolume = 0;
            }
            yield return null;
        }

        isLerpingFade = false;

        if (audioSource)
        {
            audioSource.Stop();
        }
    }

    public GameObject PlaySFX(AudioClip soundToPlay, float relativeVolume, Vector3 position, string soundTag = null, float reverb = 0, float spacial = 0)
    {
        GameObject sfxPlayer = Instantiate(sfxPlayerPrefab, position, Quaternion.identity);
        sfxPlayer.name = sfxPlayer.name + "_" + soundToPlay.name;
        if (soundTag != null)
        {
            sfxPlayer.tag = soundTag;
        }
        AudioSource sfxPlayerSource = sfxPlayer.GetComponent<AudioSource>();
        sfxPlayerSource.volume = GetGlobalSFXVolume(relativeVolume);
        sfxPlayerSource.reverbZoneMix = reverb;
        sfxPlayerSource.spatialBlend = spacial;
        sfxPlayerSource.PlayOneShot(soundToPlay);
        sfxPlayers.Add(sfxPlayerSource);
        return sfxPlayer;
    }

    public float GetGlobalSFXVolume(float relativeVolume)
    {
        return relativeVolume * gameVars.sfxVolumeScale;
    }

    [System.Serializable]
    public class MusicTrack
    {
        #region Variables & Inspector Options
        public AudioClip musicClip;
        public string musicIdentifier;
        public float musicRelativeVolume;
        #endregion      
    }
}
