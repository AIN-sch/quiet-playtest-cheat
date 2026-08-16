using System.Collections.Generic;
using UnityEngine;

namespace QUIETCheat
{
    /// <summary>一键吸取：实体物品吸到脚下（上限 SuckMax，任务优先）。房主直移同步；客端伪造吸收/抓取请求。</summary>
    public static class Suck
    {
        private static float _nextPull;
        private static float _nextSend;
        private static float _nextNotify;
        private static int _pulled;         // 本次开启累计已吸数量（按物品去重）
        private static readonly HashSet<ulong> _pulledIds = new HashSet<ulong>();  // 已吸物品网络ID，防重复计数
        private static bool _wasActive;
        private static bool _hadMission;   // 上一帧是否还有未完成任务目标（任务完成过渡用）

        public static void Update()
        {
            bool active = Features.Suck;
            if (active && !_wasActive)      // 重新开启，重置计数
            {
                _pulled = 0;
                _pulledIds.Clear();
                _nextPull = 0;
                _nextNotify = 0;
            }
            _wasActive = active;
            if (!active) return;

            var local = Features.Local;
            if (!local) return;

            // 任务完成时清计数，否则非任务物品被旧账拦着吸不动
            bool hasMission = Features.MissionTypes.Count > 0;
            if (_hadMission && !hasMission)
            {
                _pulled = 0;
                _pulledIds.Clear();
            }
            _hadMission = hasMission;

            // 达上限自动停
            if (_pulled >= Features.SuckMax)
            {
                Features.Suck = false;
                Features.Notify("已达吸取上限 " + Features.SuckMax + " 件，已自动停止");
                return;
            }

            if (Features.IsHost) HostPull(local);
            else ClientRequest(local);
        }

        /// <summary>可吸物品：网络同步 + 刚体，排除玩家/怪物/操作类交互物。</summary>
        private static bool IsItem(NetworkSyncObject sync)
        {
            if (sync == null) return false;
            if (sync.GetComponent<ActorEntity>() != null) return false;
            if (sync.GetComponent<MonsterEntity>() != null) return false;
            if (sync.GetComponent<Rigidbody>() == null) return false;
            // 交互物（按钮/门/3D UI）吸走任务会卡，排除
            if (sync.GetComponent<InteractableButton>() != null) return false;
            if (sync.GetComponent<InteractableDoor>() != null) return false;
            if (sync.GetComponent<Interactable3DUIButton>() != null) return false;
            return true;
        }

        /// <summary>房主：实体物品吸到玩家周围，螺旋摆放（一圈20个逐圈扩大），单tick限60件。</summary>
        private static void HostPull(ActorEntity local)
        {
            if (Time.time < _nextPull) return;
            _nextPull = Time.time + 0.15f;

            var pos = local.transform.position;
            var all = Object.FindObjectsByType<NetworkSyncObject>(FindObjectsSortMode.None);
            if (all.Length == 0) return;

            int batchCap = Mathf.Min(60, Features.SuckMax - _pulled);
            int batch = 0;
            for (int pass = 0; pass < 2 && batch < batchCap; pass++)   // 先任务目标再其余
            {
                foreach (var sync in all)
                {
                    if (!IsItem(sync)) continue;
                    if (batch >= batchCap) break;
                    try
                    {
                        var st = sync.Stealable;
                        bool isMission = st != null && Features.IsMissionTarget(st._type);
                        if (pass == 0 && !isMission) continue;
                        if (pass == 1 && isMission) continue;

                        // 先判任务目标再记账，否则第1轮被旧账挡掉
                        if (sync.HasId && !_pulledIds.Add(sync.ObjectId)) continue;   // 同一件不重复吸

                        float ring = Mathf.Floor(batch / 20f);
                        int inRing = batch % 20;
                        float ang = inRing * 18f * Mathf.Deg2Rad;
                        float rad = 0.3f + ring * 0.4f;
                        var target = pos
                            + Vector3.up * (0.25f + ring * 0.25f)
                            + new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad);

                        var t = sync.transform;
                        sync.ForceSnap(target, t.rotation);

                        var rb = sync.GetComponent<Rigidbody>();
                        if (rb != null && !rb.isKinematic) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

                        _pulled++; batch++;
                    }
                    catch { /* 单件异常，跳过 */ }
                }
            }
            NotifyProgress(batch);
        }

        /// <summary>客端：伪造吸收/抓取请求，隔空吸。</summary>
        private static void ClientRequest(ActorEntity local)
        {
            if (Time.time < _nextSend) return;
            _nextSend = Time.time + 0.3f;

            var nm = NetManager.Instance;
            if (nm == null || nm.API == null) return;
            byte myIndex = local.PlayerInfo.PlayerIndex;

            int batchCap = Mathf.Min(12, Features.SuckMax - _pulled);
            int batch = 0;
            for (int pass = 0; pass < 2 && batch < batchCap; pass++)   // 先任务目标再其余
            {
                foreach (var sync in Object.FindObjectsByType<NetworkSyncObject>(FindObjectsSortMode.None))
                {
                    if (!IsItem(sync) || !sync.HasId) continue;
                    if (batch >= batchCap) break;
                    try
                    {
                        var st = sync.Stealable;
                        bool isMission = st != null && Features.IsMissionTarget(st._type);
                        if (pass == 0 && !isMission) continue;
                        if (pass == 1 && isMission) continue;

                        if (!_pulledIds.Add(sync.ObjectId)) continue;   // 同一件不重复发/计数

                        nm.API.SendObjectAbsorbing(myIndex, sync.ObjectId);   // 游戏自带的"吸收"请求
                        nm.API.SendExecuteGrabbedObject(myIndex, sync.ObjectId, 0.05f, Vector3.zero);
                        _pulled++; batch++;
                    }
                    catch { /* 单件异常，跳过 */ }
                }
            }
            NotifyProgress(batch);
        }

        /// <summary>进度提示（限频每秒一次）。</summary>
        private static void NotifyProgress(int batch)
        {
            if (batch <= 0) return;
            if (Time.time < _nextNotify) return;
            _nextNotify = Time.time + 1f;
            Features.Notify("一键吸取：吸到 " + _pulled + " / " + Features.SuckMax + " 件");
        }
    }
}
