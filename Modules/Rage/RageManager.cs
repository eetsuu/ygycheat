using System;
using System.Linq;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class RageManager
    {
        public static bool AutoShoot        = false;
        public static bool AutoStop         = false;
        public static bool AutoScope        = false;
        public static bool ResolverEnabled  = false;
        public static bool TeamCheck        = true;
        public static bool VisibleCheck     = false;

        public static int   TargetBone      = 2;
        public static float Hitchance       = 50f;

        public static readonly string[] BoneNames =
        {
            "Head", "Neck", "Chest", "Pelvis"
        };

        // Calculates required aim angles from \"from\" to \"to\"
        private static Vector3 Calculate(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            float hyp = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            return new Vector3
            {
                X = MathF.Atan2(-delta.Z, hyp) * (180f / MathF.PI),
                Y = MathF.Atan2(delta.Y, delta.X) * (180f / MathF.PI),
                Z = 0f
            };
        }

        // Converts raw float[16] matrix to Matrix4x4
        private static Matrix4x4 ConvertToMatrix(float[] m)
        {
            if (m == null || m.Length < 16)
                return Matrix4x4.Identity;

            return new Matrix4x4(
                m[0],  m[1],  m[2],  m[3],
                m[4],  m[5],  m[6],  m[7],
                m[8],  m[9],  m[10], m[11],
                m[12], m[13], m[14], m[15]
            );
        }

        // World-to-screen conversion for 3D point to 2D screen space
        public static Vector2 WorldToScreen(Matrix4x4 viewMatrix, Vector3 worldPos, Vector2 screenSize)
        {
            Vector4 clip = Vector4.Transform(new Vector4(worldPos, 1f), viewMatrix);
            if (clip.W < 0.001f)
                return Vector2.Zero;

            Vector3 ndc;
            ndc.X = clip.X / clip.W;
            ndc.Y = clip.Y / clip.W;
            ndc.Z = clip.Z / clip.W;

            Vector2 screen;
            screen.X = (screenSize.X * 0.5f) * (ndc.X + 1f);
            screen.Y = (screenSize.Y * 0.5f) * (1f - ndc.Y);

            return screen;
        }

        // Retrieves bone position based on selected TargetBone
        public static Vector3 GetBone(Entity e)
        {
            if (e.Bones == null || e.Bones.Count == 0)
                return e.Head;

            if (TargetBone >= e.Bones.Count)
                return e.Bones[2];

            return e.Bones[TargetBone];
        }

        // Finds best target based on FOV, visibility, etc.
        public static Entity? FindTarget(float maxFov)
        {
            var lp = GameState.LocalPlayer;
            if (lp == null || lp.Health <= 0)
                return null;

            Entity? best       = null;
            float   bestFov    = maxFov;

            float[] rawMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            Matrix4x4 vm       = ConvertToMatrix(rawMatrix);

            foreach (var e in GameState.Entities)
            {
                if (e == null || e.Health <= 0)
                    continue;
                if (TeamCheck && e.Team == lp.Team)
                    continue;

                Vector3 bonePos = GetBone(e);
                if (bonePos == Vector3.Zero)
                    continue;

                Vector2 bone2d = WorldToScreen(vm, bonePos, GameState.renderer.screenSize);
                if (bone2d == Vector2.Zero)
                    continue;

                Vector3 aimAngle = Calculate(lp.Head, bonePos);
                float fov = AimbotMath.FovTo(lp.ViewAngles, aimAngle);

                if (fov < bestFov)
                {
                    if (VisibleCheck && !e.Visible)
                        continue;

                    best     = e;
                    bestFov  = fov;
                }
            }

            return best;
        }
    }

    public static class AimbotMath
    {
        public static Vector3 CalcAngle(Vector3 src, Vector3 dst)
        {
            Vector3 delta = dst - src;
            float hyp     = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            return new Vector3(
                (float)(Math.Atan2(-delta.Z, hyp) * 180.0f / Math.PI),
                (float)(Math.Atan2(delta.Y, delta.X) * 180.0f / Math.PI),
                0f
            );
        }

        public static float FovTo(Vector3 viewAngle, Vector3 aimAngle)
        {
            Vector3 delta = aimAngle - viewAngle;
            delta = Normalize(delta);

            return Math.Abs(delta.X) + Math.Abs(delta.Y);
        }

        public static Vector3 Normalize(Vector3 v)
        {
            if (v.X > 89f) v.X = 89f;
            if (v.X < -89f) v.X = -89f;
            while (v.Y > 180f) v.Y -= 360f;
            while (v.Y < -180f) v.Y += 360f;

            v.Z = 0f;
            return v;
        }
    }
}
