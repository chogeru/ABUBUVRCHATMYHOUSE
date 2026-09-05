using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections.Generic;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]

public class JoinLog : UdonSharpBehaviour
{
    public GameObject logTextPrefab;
    public Transform logTextContainer;
    public int maxLogLines = 10;
    public float lineOffset = 0.1f;

    private string[] logTexts;

    private void Start()
    {
        logTexts = new string[maxLogLines];
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        AddLogText("[Join] " + player.displayName);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        AddLogText("[Leave] " + player.displayName);
    }

    private void AddLogText(string text)
    {
        for (int i = logTexts.Length - 1; i > 0; i--)
        {
            logTexts[i] = logTexts[i - 1];
        }
        logTexts[0] = text;

        UpdateLogTexts();
    }

    private void UpdateLogTexts()
    {
        foreach (Transform child in logTextContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < logTexts.Length; i++)
        {
            string logText = logTexts[i];
            GameObject newLogText = VRCInstantiate(logTextPrefab);
            newLogText.transform.SetParent(logTextContainer, false);

            Vector3 positionOffset = new Vector3(0f, -i * lineOffset, 0f);
            newLogText.transform.localPosition += positionOffset;

            newLogText.GetComponent<UnityEngine.UI.Text>().text = logText;
        }
    }
}