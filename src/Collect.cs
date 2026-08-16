using UnityEngine;

namespace QUIETCheat
{
    /// <summary>一键收集。房主装包；客端伪造抓取请求隔空收（距离自己上报，谎报 0.05m）。</summary>
    public static class Collect
    {
        private static float _nextPull;
        private static float _nextSend;

        public static void Update()
        {
            if (!Features.Collect) return;
            var local = Features.Local;
            if (!local) return;

            if (Features.IsHost) HostPull(local);
            else ClientRequest(local);
        }

        /// <summary>房主：可收物装进背包（重量制，满自停），任务物品优先。</summary>
        private static void HostPull(ActorEntity local)
        {
            if (Time.time < _nextPull) return;
            _nextPull = Time.time + 0.25f;

            var inv = local.Inventory;
            if (inv == null) return;

            var all = Object.FindObjectsByType<InteractableGrabbableBase>(FindObjectsSortMode.None);
            if (all.Length == 0) return;

            int added = 0;
            bool full = false;
            for (int pass = 0; pass < 2 && !full; pass++)   // 任务目标优先
            {
                foreach (var obj in all)
                {
                    if (obj == null || !obj.IsKeepable) continue;
                    try
                    {
                        var keep = obj.GetComponent<IKeepable>();   // 基类 interop 没带 IKeepable
                        if (keep == null) continue;

                        var st = obj as InteractableStealable;
                        bool isMission = st != null && Features.IsMissionTarget(st._type);
                        if (pass == 0 && !isMission) continue;
                        if (pass == 1 && isMission) continue;

                        if (inv._curBagWeight >= inv._maxBagWeight - 0.01f) { full = true; break; }
                        inv.AddItemToInventory(keep); added++;
                    }
                    catch { /* 单件装包异常，跳过 */ }
                }
            }
            if (added > 0) Features.Notify("一键收集：装包 " + added + " 件");
        }

        /// <summary>客端：伪造抓取请求，隔空收。</summary>
        private static void ClientRequest(ActorEntity local)
        {
            if (Time.time < _nextSend) return;
            _nextSend = Time.time + 0.35f;

            var nm = NetManager.Instance;
            if (nm == null || nm.API == null) return;
            byte myIndex = local.PlayerInfo.PlayerIndex;

            int sent = 0;
            foreach (var obj in Object.FindObjectsByType<InteractableGrabbableBase>(FindObjectsSortMode.None))
            {
                if (obj == null) continue;
                var sync = obj.GetComponent<NetworkSyncObject>();
                if (sync == null || !sync.HasId) continue;
                nm.API.SendExecuteGrabbedObject(myIndex, sync.ObjectId, 0.05f, Vector3.zero);
                sent++;
                if (sent >= 8) break;
            }
            if (sent > 0) Features.Notify("一键收集(客端)：发送 " + sent + " 个抓取请求");
        }
    }
}
