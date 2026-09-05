// DoorTeleporter_PropsWithBeginnerHeaders.cs
// UdonSharp / VRChat
// 「ヘッダのみ」「初心者向け短文＋章番号」で各プロパティを説明

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DoorCSharpProgramAsset : UdonSharpBehaviour
{
    [Header("① 行き先（Transform）— ここへテレポート")]
    public Transform target = null;

    [Header("② 行き先を固定 — ONで起動時の位置に固定 / OFFで常に現在")]
    public bool lockTargetSnapshot = false;

    [Header("③ 連打防止（秒）— 次に実行できるまでの待ち時間")]
    public float cooldownSec = 0.35f;

    [Header("④ 地面に合わせる — ONで床の高さに着地")]
    public bool alignToGround = true;

    [Header("⑤ 地面判定の距離（m）— ④で下に探す長さ")]
    public float groundRayDistance = 3.0f;

    [Header("⑥ 前方オフセット（m）— 到着時に少し前へ出す")]
    public float forwardSafetyOffset = 0.06f;

    [Header("⑦ 近すぎる時は回転のみ（m）— 位置は据え置き")]
    public float minMoveDistance = 0.15f;

    [Header("⑧ 遠すぎる時は実行しない（m・0で無制限）")]
    public float maxMoveDistance = 0.0f;

    [Header("⑨ マスターのみ実行 — ONで制限")]
    public bool masterOnly = false;

    [Header("⑩ 成功時のSE（任意）— 未設定なら無音")]
    public AudioSource sfxOnTeleport = null;

    // 内部変数（インスペクタに出さない）
    private Vector3 _snapPos;
    private Quaternion _snapRot;
    private float _lastUsedAt;

    void Start()
    {
        if (!Utilities.IsValid(target)) target = transform;

        if (lockTargetSnapshot)
        {
            _snapPos = target.position;
            _snapRot = target.rotation;
        }

        if (cooldownSec < 0f) cooldownSec = 0f;
        if (groundRayDistance < 0.1f) groundRayDistance = 0.1f;
        if (forwardSafetyOffset < 0f) forwardSafetyOffset = 0f;
        if (minMoveDistance < 0f) minMoveDistance = 0f;
        if (maxMoveDistance < 0f) maxMoveDistance = 0f;
    }

    public override void Interact()
    {
        var me = Networking.LocalPlayer;
        if (!Utilities.IsValid(me)) return;
        if (masterOnly && !me.isMaster) return;

        float now = Time.time;
        if (now - _lastUsedAt < cooldownSec) return;
        _lastUsedAt = now;

        // 目的地（スナップ or 現在値）
        Vector3 dstPos = lockTargetSnapshot ? _snapPos : target.position;
        Quaternion dstRot = lockTargetSnapshot ? _snapRot : target.rotation;

        // 地面合わせ
        if (alignToGround)
        {
            RaycastHit hit;
            Vector3 rayStart = dstPos + Vector3.up * 0.5f;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, groundRayDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                dstPos = hit.point;
            }
        }

        // めり込み回避の前方オフセット
        if (forwardSafetyOffset > 0f)
        {
            dstPos += dstRot * Vector3.forward * forwardSafetyOffset;
        }

        // 距離ルール
        Vector3 curPos = me.GetPosition();
        float dist = Vector3.Distance(curPos, dstPos);

        if (maxMoveDistance > 0f && dist > maxMoveDistance) return;

        if (minMoveDistance > 0f && dist < minMoveDistance)
        {
            // 近すぎる場合：位置はそのまま、向きだけ合わせる
            me.TeleportTo(curPos, dstRot, VRC_SceneDescriptor.SpawnOrientation.Default, true);
        }
        else
        {
            me.TeleportTo(dstPos, dstRot, VRC_SceneDescriptor.SpawnOrientation.Default, true);
        }

        if (Utilities.IsValid(sfxOnTeleport)) sfxOnTeleport.Play();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var t = target ? target : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(t.position, 0.08f);
        Vector3 f = t.rotation * Vector3.forward * 0.4f;
        Gizmos.DrawLine(t.position, t.position + f);
    }
#endif
}
