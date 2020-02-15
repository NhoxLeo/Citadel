using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using UnityEngine.SceneManagement;

public class LevelClearTrigger : MonoBehaviour
{
    private bool hasBeenTriggered;
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 8 && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            StartCoroutine(CompleteLevel());
        }
    }

    private IEnumerator CompleteLevel()
    {
        InteractionController.instance.hasPlayerDied = true;
        InteractionController.instance.playerDamageAnimator.SetBool("HasDied", true);

        InteractionController.instance.switchingWeapons = true;
        InteractionController.instance.weaponStorageParent.GetComponent<Animator>().SetBool("Equip", false);
        InteractionController.instance.weaponStorageParent.GetComponent<Animator>().SetBool("Unequip", true);

        AudioSource[] audioSources = GameObject.FindObjectsOfType<AudioSource>();
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                if (audioSources[i].gameObject.transform.parent != gameObject.transform.parent)
                {
                    StartCoroutine(GameVars.instance.audioManager.FadeOutAudioTrack(audioSources[i], 7));
                }
            }
        }

        GameVars.instance.totalTimeSpent = Time.timeSinceLevelLoad;
        GameVars.instance.wasLevelBeaten = true;

        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Level Clear");
    }
}
