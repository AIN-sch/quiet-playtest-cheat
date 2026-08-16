using System.Collections.Generic;
using DataTable;
using UnityEngine;

namespace QUIETCheat
{
    /// <summary>全局状态 + 面板 + 通用工具。</summary>
    public static class Features
    {
        public static bool MenuOpen = true;

        // ---- 功能开关（范围见菜单文案）----
        public static bool God = false;            // 无敌锁血 + 无限体力
        public static bool Noclip = false;         // 穿墙/飞行（房主）
        public static bool FreezeMonsters = false; // 怪物定身（房主）
        public static bool NoAlert = false;        // 无警报锁阶段（房主）
        public static bool Collect = false;        // 一键收集
        public static bool Suck = false;           // 一键吸取
        public static int SuckMax = 100;           // 吸取上限（吸满自动停，防卡）
        public static bool Esp = true;             // ESP 透视

        // ---- 角色检测（房主 / 客户端）----
        private enum Role { None, Host, Client }
        private static Role _lastRole = Role.None;

        public const float NoclipSpeed = 8f;

        // ---- 面板提示（显示最后一条）----
        private static string _lastMsg = "";
        private static float _lastMsgTime;

        public static void Notify(string msg)
        {
            _lastMsg = msg;
            _lastMsgTime = Time.time;
            if (Plugin.Logger != null) Plugin.Logger.LogInfo(msg);
        }

        /// <summary>本地角色；不在对局时为 null。</summary>
        public static ActorEntity Local => StageDataStorage.MyEntity;

        /// <summary>是否房主（服务端权威）。</summary>
        public static bool IsHost
        {
            get
            {
                var nm = NetManager.Instance;
                return nm != null && nm.IsHost;
            }
        }

        // ---- 任务目标物品类型（每秒刷新，收集/吸取优先）----
        private static readonly HashSet<StealableType> _missionTypes = new HashSet<StealableType>();
        private static float _nextMissionScan;

        /// <summary>当前未完成任务的物品类型集合（没进任务/全完成 = 空）。</summary>
        public static HashSet<StealableType> MissionTypes
        {
            get
            {
                if (Time.time >= _nextMissionScan)
                {
                    _nextMissionScan = Time.time + 1f;
                    _missionTypes.Clear();
                    try
                    {
                        var sm = StageManager.Instance;
                        var mission = sm != null ? sm.Mission : null;
                        if (mission != null)
                        {
                            int n = mission.MissionCount;
                            for (int i = 0; i < n; i++)
                            {
                                var cat = mission.GetMissionData(i);
                                if (cat == null || cat.IsCompleted) continue;
                                foreach (var md in cat.MissionList)
                                {
                                    if (md == null || md.IsCompleted) continue;
                                    var t = md.Table.StealableType;
                                    if (t != StealableType.None) _missionTypes.Add(t);
                                }
                            }
                        }
                    }
                    catch { /* 任务数据未就绪，跳过本轮 */ }
                }
                return _missionTypes;
            }
        }

        /// <summary>该物品类型是否属于当前任务目标。</summary>
        public static bool IsMissionTarget(StealableType type)
        {
            return type != StealableType.None && MissionTypes.Contains(type);
        }

        public static void Tick()
        {
            DetectRole();
        }

        /// <summary>身份检测：MyEntity 存在=在对局，IsHost 判房主/客户端，变化时提示并清状态。</summary>
        private static void DetectRole()
        {
            Role now;
            if (StageDataStorage.MyEntity == null)
            {
                now = Role.None;   // 未进对局
            }
            else
            {
                var nm = NetManager.Instance;
                now = nm != null && nm.IsHost ? Role.Host : Role.Client;
            }

            if (now == _lastRole) return;
            _lastRole = now;

            switch (now)
            {
                case Role.Host:
                    Notify("房主模式：全功能可用");
                    break;
                case Role.Client:
                    God = Noclip = FreezeMonsters = NoAlert = Collect = Suck = false;  // 只留 ESP
                    Notify("当前为客户端：仅 ESP 可用");
                    break;
                default:
                    _lastMsg = "";
                    _lastMsgTime = -1000f;
                    break;
            }
        }

        // ---------------- IMGUI 面板 ----------------
        public static class Menu
        {
            private static Rect _win = new Rect(20f, 20f, 340f, 460f);   // 不能 readonly，拖拽后要存回新矩形
            private static string _suckMaxText = "100";   // 吸取上限输入框文本
            private static bool _suckFocus;               // 输入框聚焦时键盘输入才生效
            private static readonly GUI.WindowFunction _winFunc =   // IL2CPP 下方法组要包一层 Action<int>
                (GUI.WindowFunction)new System.Action<int>(DoWindow);

            public static void Draw()
            {
                _win = GUI.Window(9017, _win, _winFunc, "QUIET Cheat v1.0.0");
            }

            private static void DoWindow(int id)
            {
                GUILayout.BeginVertical();

                bool host = Features.IsHost;
                bool inGame = Features.Local != null;

                GUILayout.Label(host
                    ? "房主：全功能可用"
                    : (inGame ? "当前为客户端" : "未进入对局"));

                GUI.enabled = host;   // 房主限定，客户端置灰
                Features.God = GUILayout.Toggle(Features.God, "无敌锁血 + 无限体力");
                Features.Collect = GUILayout.Toggle(Features.Collect, "一键收集（装背包）");
                Features.Suck = GUILayout.Toggle(Features.Suck, "一键吸取（吸到脚下）");
                // 吸取上限输入框：IL2CPP 无 DoTextField，自绘收键盘输入
                GUILayout.BeginHorizontal();
                GUILayout.Label("吸取上限(防卡)", GUILayout.Width(96));
                Rect fld = GUILayoutUtility.GetRect(52f, 22f, GUILayout.Width(52f));
                if (Event.current.type == EventType.MouseDown)
                {
                    _suckFocus = fld.Contains(Event.current.mousePosition);
                    if (_suckFocus) Event.current.Use();
                }
                GUI.Box(fld, _suckMaxText + (_suckFocus ? "▏" : ""));
                if (_suckFocus && Event.current.isKey && Event.current.type == EventType.KeyDown)
                {
                    char c = Event.current.character;
                    if (char.IsDigit(c) && _suckMaxText.Length < 4)
                        _suckMaxText += c;
                    else if (Event.current.keyCode == KeyCode.Backspace && _suckMaxText.Length > 0)
                        _suckMaxText = _suckMaxText.Substring(0, _suckMaxText.Length - 1);
                    else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape)
                        _suckFocus = false;
                }
                if (GUILayout.Button("-10", GUILayout.Width(34)))
                    _suckMaxText = Mathf.Clamp(Features.SuckMax - 10, 1, 2000).ToString();
                if (GUILayout.Button("+10", GUILayout.Width(34)))
                    _suckMaxText = Mathf.Clamp(Features.SuckMax + 10, 1, 2000).ToString();
                if (int.TryParse(_suckMaxText, out int v))
                    Features.SuckMax = Mathf.Clamp(v, 1, 2000);
                GUILayout.EndHorizontal();

                Features.Noclip = GUILayout.Toggle(Features.Noclip, "穿墙 / 飞行（WASD+空格上/ctrl下）");
                Features.FreezeMonsters = GUILayout.Toggle(Features.FreezeMonsters, "怪物定身");
                Features.NoAlert = GUILayout.Toggle(Features.NoAlert, "无警报 · 锁阶段");
                GUI.enabled = true;

                // 双方都可用
                Features.Esp = GUILayout.Toggle(Features.Esp, "ESP 透视");

                if (Time.time - _lastMsgTime < 6f)
                    GUILayout.Label("提示: " + _lastMsg);

                GUILayout.Label("INS 开关面板 · Bug懒得修 · 项目已开源 · 出了问题自己修");
                GUILayout.Label("QQ技术讨论群(多多少少都不爱说话)：1102821216");

                GUILayout.EndVertical();
                GUI.DragWindow();
            }
        }
    }
}
