using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace QUIETCheat
{
    /// <summary>插件入口（BepInEx 6 IL2CPP）。</summary>
    [BepInPlugin("quieter.cheat", "QUIET Cheat", "1.0.0")]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource Logger;

        public override void Load()
        {
            Logger = base.Log;
            AddComponent<CheatBehaviour>();
            Harmony.CreateAndPatchAll(GetType().Assembly);
            Logger.LogInfo("QUIET Cheat v1.0.0 loaded. INS = 面板开关");
        }
    }

    /// <summary>游戏内行为组件：每帧推进各功能模块 + IMGUI 菜单/ESP。</summary>
    public class CheatBehaviour : MonoBehaviour
    {
        // 光标状态备份（打开/关闭面板时用）
        private static CursorLockMode _savedLock = CursorLockMode.Locked;
        private static bool _savedVisible;
        private static bool _cursorTaken;

        private void Update()
        {
            // INS 开关面板
            var kbd = UnityEngine.InputSystem.Keyboard.current;
            if (kbd != null && kbd[UnityEngine.InputSystem.Key.Insert].wasPressedThisFrame)
            {
                Features.MenuOpen = !Features.MenuOpen;
                ApplyCursorMenu();
            }

            // 菜单开着时每帧解锁鼠标，否则游戏锁着没法点面板
            if (Features.MenuOpen && Features.Local != null)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Features.Tick();
            God.Update();
            Noclip.Update();
            Freeze.Update();
            Collect.Update();
            Suck.Update();
            PhaseLock.Update();
            Esp.Update();
        }

        /// <summary>光标接管/恢复（面板开时接管，关时还原）。</summary>
        private static void ApplyCursorMenu()
        {
            if (Features.MenuOpen)
            {
                _savedLock = Cursor.lockState;
                _savedVisible = Cursor.visible;
                _cursorTaken = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (_cursorTaken)
            {
                _cursorTaken = false;
                Cursor.lockState = _savedLock;
                Cursor.visible = _savedVisible;
            }
        }

        private void OnGUI()
        {
            if (Features.MenuOpen) Features.Menu.Draw();
            Esp.Draw();
        }
    }
}
