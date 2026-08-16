using UnityEngine;
using UnityEngine.InputSystem;

namespace QUIETCheat
{
    /// <summary>穿墙 / 飞行（仅房主）。关碰撞 + 停游戏移动，直接改 transform；客端移动是房主算的，改不了。</summary>
    public static class Noclip
    {
        private static bool _wasActive;

        public static void Update()
        {
            var local = Features.Local;
            bool active = Features.Noclip && Features.IsHost && local;

            if (!active)
            {
                if (_wasActive)
                {
                    // 恢复游戏自带移动 + 碰撞/重力
                    var mv = local ? local.Movement : null;
                    if (mv != null) mv.enabled = true;

                    var rb = local ? local.Rigidbody : null;
                    if (rb != null)
                    {
                        rb.detectCollisions = true;
                        if (!rb.isKinematic) rb.useGravity = true;
                    }
                    _wasActive = false;
                }
                return;
            }
            _wasActive = true;

            // 停掉游戏自带移动，防它每帧把人拉回去
            var move = local.Movement;
            if (move != null) move.enabled = false;

            var rigid = local.Rigidbody;
            if (rigid == null) return;
            rigid.detectCollisions = false;
            if (!rigid.isKinematic)
            {
                rigid.useGravity = false;
                // 清掉残留速度，防关重力后惯性把人顶飞
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
            }

            // 菜单开着不操控，防在面板上按 WASD 把人带走
            Vector3 dir = Vector3.zero;
            if (!Features.MenuOpen)
            {
                var kbd = Keyboard.current;
                var cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
                if (kbd != null)
                {
                    Vector3 fwd = cam ? cam.transform.forward : Vector3.forward;
                    Vector3 right = cam ? cam.transform.right : Vector3.right;
                    fwd.y = 0f; fwd.Normalize();
                    right.y = 0f; right.Normalize();

                    if (kbd[Key.W].isPressed) dir += fwd;
                    if (kbd[Key.S].isPressed) dir -= fwd;
                    if (kbd[Key.D].isPressed) dir += right;
                    if (kbd[Key.A].isPressed) dir -= right;
                    if (kbd[Key.Space].isPressed) dir += Vector3.up;
                    if (kbd[Key.LeftCtrl].isPressed || kbd[Key.C].isPressed) dir -= Vector3.up;
                }
            }
            if (dir.sqrMagnitude > 0.001f) dir.Normalize();

            local.transform.position += dir * (Features.NoclipSpeed * Time.deltaTime);
        }
    }
}
