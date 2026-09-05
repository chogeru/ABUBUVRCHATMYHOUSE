using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]

public class CQ_Clock : UdonSharpBehaviour
{
    public UnityEngine.UI.Text dateText;
    public UnityEngine.UI.Text timeText;
    public UnityEngine.UI.Text dayText;

    private float updateInterval = 60f; // 更新間隔（秒）
    private float dayOfWeekUpdateInterval = 60f; // 曜日の更新間隔（秒）

    private void Start()
    {
        UpdateDateTime();
        UpdateDayOfWeek();
        // SendCustomEventDelayedSeconds(nameof(UpdateDateTime), updateInterval);
        // SendCustomEventDelayedSeconds(nameof(UpdateDayOfWeek), dayOfWeekUpdateInterval);
    }

    public void UpdateDateTime()
    {
        System.DateTime now = System.DateTime.Now;
        dateText.text = now.ToString("yyyy/MM/dd");
        timeText.text = now.ToString("HH:mm");
        SendCustomEventDelayedSeconds(nameof(UpdateDateTime), updateInterval);
    }

    public void UpdateDayOfWeek()
    {
        System.DateTime now = System.DateTime.Now;
        dayText.text = now.ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        SendCustomEventDelayedSeconds(nameof(UpdateDayOfWeek), dayOfWeekUpdateInterval);
    }
}