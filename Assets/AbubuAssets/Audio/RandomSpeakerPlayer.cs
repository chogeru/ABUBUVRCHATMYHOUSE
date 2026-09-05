using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class RandomSpeakerPlayer : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] clips;

    private int lastIndex = -1;

    void Start()
    {
        PlayRandomClip();
    }

    void Update()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            PlayRandomClip();
        }
    }

    private void PlayRandomClip()
    {
        if (audioSource == null || clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);
        if (clips.Length > 1)
        {
            while (index == lastIndex)
            {
                index = Random.Range(0, clips.Length);
            }
        }
        lastIndex = index;

        audioSource.clip = clips[index];
        audioSource.Play();
    }
}
