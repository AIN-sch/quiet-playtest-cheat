using HarmonyLib;
using UnityEngine;

namespace QUIETCheat
{
    /// <summary>无敌锁血 + 无限体力：每帧血量/体力写满（房主真无敌，客端防普通攻击）。</summary>
    public static class God
    {
        public static void Update()
        {
            if (!Features.God) return;
            var local = Features.Local;
            if (!local) return;
            var vital = local.Vital;
            if (vital == null) return;

            vital._curHealth = vital._maxHealth;
            vital._curStamina = vital._maxStamina;
        }
    }

    /// <summary>拦掉伤害结算入口。</summary>
    [HarmonyPatch(typeof(ActorVital), "OnAttack")]
    public static class Patch_GodOnAttack
    {
        static bool Prefix()
        {
            return !Features.God;
        }
    }

    /// <summary>拦掉血线设置/同步，防血量被盖回去。</summary>
    [HarmonyPatch(typeof(ActorVital), "ApplyHealth")]
    public static class Patch_GodApplyHealth
    {
        static bool Prefix()
        {
            return !Features.God;
        }
    }
}
