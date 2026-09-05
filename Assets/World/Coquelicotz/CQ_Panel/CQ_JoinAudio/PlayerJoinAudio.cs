using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]

public class PlayerJoinAudio : UdonSharpBehaviour
{
    public AudioSource audioSource; // 再生するオーディオソース

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}