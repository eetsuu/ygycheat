using System;
using System.Linq;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public class SilentAim : ThreadService
    {
        public static bool Enabled = false;
        public static float FOV = 10f;
        public static bool TeamCheck = true;

        protected override void FrameAction()
        {
            if (!Enabled)
                return;

            var local = GameState.LocalPlayer;
            if (local == null || local.Health <= 0)
                return;

            // Update global target
            RageManager.UpdateTarget();

            // If no target, skip
            if (RageManager.CurrentTarget == null)
                return;

            var target = RageManager.CurrentTarget;

            // FOV check
            if (GetFov(local, target) > FOV)
                return;

            // AutoWall filter
            if (!RageManager.CanHitTarget())
                return;

            // Aim at shared bone
            Vector3 aimPoint = RageManager.GetAimPoint();
            Vector3 desiredAngle = CalcAngle(local, aimPoint);

            ApplySilentAngles(desiredAngle);
        }

        private float GetFov(Entity local, Entity enemy)
        {
            Vector3 src = local.Position + local.View;
            Vector3 dst = enemy.Position + enemy.View;

            Vector3 ang = CalcAngle(local, dst);
            Vector3 delta = ang - local.ViewAngles;

            Normalize(ref delta);
            return delta.Length();
        }

        private Vector3 CalcAngle(Entity local, Vector3 target)
        {
            Vector3 src = local.Position + local.View;
            Vector3 delta = target - src;

            float hyp = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            Vector3 ang;
            ang.X = -MathF.Atan2(delta.Z, hyp) * 57.29578f;
            ang.Y = MathF.Atan2(delta.Y, delta.X) * 57.29578f;
            ang.Z = 0;

            Normalize(ref ang);
            return ang;
        }

        private void Normalize(ref Vector3 ang)
        {
            if (ang.X > 89f) ang.X = 89f;
            if (ang.X < -89f) ang.X = -89f;

            while (ang.Y > 180f) ang.Y -= 360f;
            while (ang.Y < -180f) ang.Y += 360f;

            ang.Z = 0;
        }

        private void ApplySilentAngles(Vector3 angle)
        {
            // External silent aim through input buffer
            IntPtr input = GameState.swed.ReadPointer(GameState.client + Offsets.dwCSGOInput);
            if (input == IntPtr.Zero)
                return;

            IntPtr anglePtr = input + Offsets.dwViewAngles;
            GameState.swed.WriteVec(anglePtr, angle);
        }
    }
}
