using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSoundsManager : MonoBehaviour
{
    public List<MoveAudioList> moveAudioLists;

    public MoveAudioList FindSurfaceAudio(string surfaceTagToFind)
    {
        foreach(MoveAudioList moveAudioList in moveAudioLists)
        {
            if(moveAudioList.surfaceTag == surfaceTagToFind)
            {
                return moveAudioList;
            }
        }
        return moveAudioLists[0];
    }

    public AudioClip GetImpactSound(MoveAudioList surfaceAudioList)
    {
        if(surfaceAudioList.moveAudioList != null && surfaceAudioList.moveAudioList.Count > 0)
        {
            return surfaceAudioList.moveAudioList[0].GetClipToPlay();
        }
        else
        {
            return null;
        }
    }

    public AudioClip GetFootSepsSound(MoveAudioList surfaceAudioList)
    {
        if (surfaceAudioList.moveAudioList != null && surfaceAudioList.moveAudioList.Count > 0)
        {
            return surfaceAudioList.moveAudioList[1].GetClipToPlay();
        }
        else
        {
            return null;
        }
    }

    [System.Serializable]
    public class MoveAudioList
    {
        public string surfaceTag;
        public List<MoveAudio> moveAudioList; //Index 0 Will Always Be an Impact, Index 1 Will Always Be Footsteps
    }

    [System.Serializable]
    public class MoveAudio
    {
        [Header("Audio Source Settings")]
        #region Audio Source Settings
        [Tooltip("The audioclip to be played")]
        public AudioPlayType audioPlayType = AudioPlayType.Random;
        [Tooltip("The audioclip to be played")]
        public List<AudioClip> audioClips;
        #endregion

        #region Stored Data
        public enum AudioPlayType { Random, Increment }
        private int currentIndex = 0;
        #endregion

        public AudioClip GetClipToPlay()
        {
            AudioClip clipToReturn = null;
            if(audioPlayType == AudioPlayType.Random)
            {
                clipToReturn = audioClips[Random.Range(0, audioClips.Count)];
            }
            else if (audioPlayType == AudioPlayType.Increment)
            {
                clipToReturn = audioClips[currentIndex];
                currentIndex++;

                if(currentIndex > audioClips.Count-1)
                {
                    currentIndex = 0;
                }
            }
            return clipToReturn;
        }
    }
}


