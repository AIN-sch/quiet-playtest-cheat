using UnityEngine;

namespace QUIETCheat
{
    /// <summary>无警报 · 锁阶段（仅房主）：每帧清警报计量表，怪物停在初始阶段。</summary>
    public static class PhaseLock
    {
        public static void Update()
        {
            if (!Features.NoAlert) return;
            if (!Features.IsHost) return;
            var sm = StageManager.Instance;
            if (sm == null) return;
            var phase = sm.Phase;
            if (phase == null) return;

            phase._curGauge = 0;
        }
    }
}
