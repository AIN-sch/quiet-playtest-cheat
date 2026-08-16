using UnityEngine;

namespace QUIETCheat
{
    /// <summary>一键收集（仅房主）：可收物装进背包，任务物品优先。</summary>
    public static class Collect
    {
        private static float _nextPull;

        public static void Update()
        {
            if (!Features.Collect || !Features.IsHost) return;
            var local = Features.Local;
            if (!local) return;

            HostPull(local);
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
    }
}
