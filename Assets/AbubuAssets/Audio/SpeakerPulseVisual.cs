using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class SpeakerPulseVisual : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public Transform pulseTransform;
    public float pulseSpeed = 14f;
    public float pulseAmount = 0.06f;

    private Vector3 baseScale;

    void Start()
    {
        if (pulseTransform == null) pulseTransform = transform;
        baseScale = pulseTransform.localScale;
    }

    void Update()
    {
        if (pulseTransform == null) return;

        if (audioSource != null && audioSource.isPlaying)
        {
            float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseAmount;
            pulseTransform.localScale = baseScale * pulse;
        }
        else
        {
            pulseTransform.localScale = baseScale;
        }
    }
}
