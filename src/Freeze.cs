using HarmonyLib;
using UnityEngine;

namespace QUIETCheat
{
    /// <summary>怪物定身（仅房主）：跳过怪物 AI 帧，原地定住。</summary>
    public static class Freeze
    {
        public static void Update()
        {
            // 由下面 Harmony Prefix 拦，无需做事
        }
    }

    /// <summary>定身时跳过物理/AI 帧。</summary>
    [HarmonyPatch(typeof(MonsterBehaviour), "FixedUpdate")]
    public static class Patch_FreezeMonsterFixedUpdate
    {
        static bool Prefix()
        {
            return !(Features.FreezeMonsters && Features.IsHost);
        }
    }

    /// <summary>定身时跳过行为状态更新。</summary>
    [HarmonyPatch(typeof(MonsterBehaviour), "Update")]
    public static class Patch_FreezeMonsterUpdate
    {
        static bool Prefix()
        {
            return !(Features.FreezeMonsters && Features.IsHost);
        }
    }
}
