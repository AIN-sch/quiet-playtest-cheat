using System.Collections.Generic;
using UnityEngine;

namespace QUIETCheat
{
    /// <summary>ESP 透视（双方可用）。怪物中文名+距离（红），可抓物（黄）。每 0.5s 扫一次。</summary>
    public static class Esp
    {
        private struct Target
        {
            public Transform Transform;
            public string Name;
            public bool IsMonster;
        }

        private static float _nextScan;
        private static readonly List<Target> _targets = new List<Target>();
        private static readonly GUIStyle _monsterStyle = new GUIStyle();
        private static readonly GUIStyle _itemStyle = new GUIStyle();
        private static bool _stylesReady;

        public static void Update()
        {
            if (!Features.Esp) return;
            if (Time.time < _nextScan) return;
            _nextScan = Time.time + 0.5f;
            _targets.Clear();

            var sm = StageManager.Instance;
            if (sm != null && sm.Monsters != null)
            {
                foreach (var m in sm.Monsters)
                {
                    if (m == null) continue;
                    _targets.Add(new Target { Transform = m.transform, Name = Names.MonsterName(m.Type), IsMonster = true });
                }
            }

            foreach (var o in Object.FindObjectsByType<InteractableGrabbableBase>(FindObjectsSortMode.None))
            {
                if (o == null) continue;
                string name = !string.IsNullOrEmpty(o.Name) ? o.Name : "物品";
                if (o is InteractableStealable st)
                    name = Names.ItemName(st._type);
                _targets.Add(new Target { Transform = o.transform, Name = name, IsMonster = false });
            }
        }

        public static void Draw()
        {
            if (!Features.Esp || _targets.Count == 0) return;
            if (Features.Local == null) return;   // 没进对局不画

            if (!_stylesReady)
            {
                _stylesReady = true;
                _monsterStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);
                _monsterStyle.fontSize = 12;
                _monsterStyle.alignment = TextAnchor.MiddleCenter;
                _itemStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
                _itemStyle.fontSize = 12;
                _itemStyle.alignment = TextAnchor.MiddleCenter;
            }

            var cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (cam == null) return;
            Vector3 self = Features.Local.transform.position;

            foreach (var t in _targets)
            {
                if (t.Transform == null) continue;
                Vector3 w = t.Transform.position + Vector3.up * 1.6f;
                Vector3 s = cam.WorldToScreenPoint(w);
                if (s.z < 0.1f) continue;      // 相机背后不画
                s.y = Screen.height - s.y;

                float dist = Vector3.Distance(t.Transform.position, self);
                string label = t.Name + "  " + dist.ToString("0") + "m";
                GUI.Label(new Rect(s.x - 70, s.y - 10, 140, 22), label, t.IsMonster ? _monsterStyle : _itemStyle);
            }
        }
    }
}
