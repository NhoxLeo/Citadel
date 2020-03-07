using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public PlayType playType;
    public bool playOnAwake = true;
    public AudioList ambientSounds;

    private bool hasPlayed;
    private bool isStarting;
    private bool isLerpingFade;
    private int currentPlayIndex = 0;

    public enum PlayType { Sequential, Random}

    // Start is called before the first frame update
    private void Start()
    {
        if (playOnAwake)
        {
            StartPlay();
        }
    }

    private void Update()
    {
        if (hasPlayed && !audioSource.isPlaying && !GameVars.instance.isPaused)
        {
            StartPlay();
            //Debug.Log("Play New Clip");
        }
    }

    public void StartPlay()
    {
        StartCoroutine(Play());
    }

    public void StopPlay()
    {
        if (audioSource.isPlaying)
        {
            FadeOutAudioTrack(audioSource, 8.0f);
        }
    }

    private IEnumerator Play()
    {
        if (!isStarting)
        {
            isStarting = true;
            hasPlayed = false;
            StopPlay();

            yield return new WaitUntil(() => (audioSource.isPlaying == false));
            PlayAmbientAudio(ChooseAmbientAudio());
        }
    }

    private ReactionAudio ChooseAmbientAudio()
    {
        if (ambientSounds.audioList.Count > 1)
        {
            if (playType == PlayType.Random)
            {
                currentPlayIndex = Random.Range(0, ambientSounds.audioList.Count);
                return ambientSounds.audioList[currentPlayIndex];
            }
            else if (playType == PlayType.Sequential)
            {
                if (currentPlayIndex < ambientSounds.audioList.Count)
                {
                    currentPlayIndex++;
                }
                else
                {
                    currentPlayIndex = 1;
                }
                return ambientSounds.audioList[currentPlayIndex - 1];
            }
        }
        return ambientSounds.audioList[0];
    }

    private void PlayAmbientAudio(ReactionAudio chosenClip)
    {
        audioSource.Stop();
        if(chosenClip.doLoop)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
               
        float totalVolume = chosenClip.volume * GameVars.instance.musicVolumeScale;
        audioSource.volume = 0;
        audioSource.pitch = chosenClip.pitch;
        audioSource.priority = chosenClip.priority;
        audioSource.clip = chosenClip.audioClip;
        audioSource.Play();
        isStarting = false;
        hasPlayed = true;
        StartCoroutine(FadeInAudioTrack(audioSource, totalVolume, 8.0f));
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
}

[System.Serializable]
public class AudioList
{
    public List<ReactionAudio> audioList;
}

[System.Serializable]
public class ReactionAudio
{
    [Header("Audio Source Settings")]
    #region Audio Source Settings
    [Tooltip("The audioclip to be played")]
    public AudioClip audioClip;
    [Tooltip("The volume this audio will be played at (Default 1.0f)")]
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    [Tooltip("The priority this audio will be played at (Default 128)")]
    [Range(0, 256)]
    public int priority = 128;
    [Tooltip("The pitch this audio will be played at (Default 1.0f)")]
    [Range(-3.0f, 3.0f)]
    public float pitch = 1.0f;
    [Tooltip("If this audio will loop on completion")]
    public bool doLoop = false;
    #endregion
}
