using System;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class AntiAim
    {
        public static bool Enabled = false;

        public static int PitchMode = 0;
        public static int YawMode = 0;

        public static float YawAdd = 0f;
        public static float DesyncAmount = 35f;

        public static bool BreakLegs = false;
        public static bool NoDuckDelay = false;
        public static bool ThirdPerson = false;

        public static Vector3 RealAngle = Vector3.Zero;
        public static Vector3 FakeAngle = Vector3.Zero;

        private static float spin = 0f;
        private static float jitter = 1f;
        private static readonly Random rng = new();

        public static readonly string[] PitchModes =
        {
            "Off", "Up", "Down", "Zero", "Fake Up", "Fake Down"
        };

        public static readonly string[] YawModes =
        {
            "Off", "Backward", "Jitter", "Spin", "Random",
            "Desync Left", "Desync Right", "LBY", "Sideways", "180z"
        };

        public static void Run()
        {
            if (!Enabled)
                return;

            var lp = GameState.LocalPlayer;
            if (lp == null || lp.Health <= 0)
                return;

            RealAngle = lp.ViewAngles;

            FakeAngle = RealAngle;
            ApplyPitch(ref FakeAngle);
            ApplyYaw(ref FakeAngle);
            ApplyDesync(ref FakeAngle);
            Normalize(ref FakeAngle);

            if (ThirdPerson)
                WriteModelAngles(FakeAngle);

            if (BreakLegs)
                BreakMovement();

            if (NoDuckDelay)
                RemoveDuckDelay();
        }

        private static void ApplyPitch(ref Vector3 ang)
        {
            switch (PitchMode)
            {
                case 1: ang.X = -89; break;
                case 2: ang.X = 89; break;
                case 3: ang.X = 0; break;
                case 4: ang.X = -540; break;
                case 5: ang.X = 540; break;
            }
        }

        private static void ApplyYaw(ref Vector3 ang)
        {
            float baseY = ang.Y + YawAdd;

            switch (YawMode)
            {
                case 1:
                    ang.Y = baseY + 180f;
                    break;

                case 2:
                    jitter *= -1f;
                    ang.Y = baseY + (jitter > 0 ? 120f : -120f);
                    break;

                case 3:
                    spin += 3.5f;
                    if (spin >= 360) spin = 0;
                    ang.Y = baseY + spin;
                    break;

                case 4:
                    ang.Y = baseY + (float)(rng.NextDouble() * 360 - 180);
                    break;

                case 5:
                    ang.Y = baseY - DesyncAmount;
                    break;

                case 6:
                    ang.Y = baseY + DesyncAmount;
                    break;

                case 7:
                    if ((Environment.TickCount % 1000) < 150)
                        ang.Y = baseY + 120f;
                    else
                        ang.Y = baseY + 180f;
                    break;

                case 8:
                    ang.Y = baseY + 90f;
                    break;

                case 9:
                    ang.Y = baseY + ((Environment.TickCount % 200 < 100) ? 180f : -180f);
                    break;
            }

            Normalize(ref ang);
        }

        private static void ApplyDesync(ref Vector3 ang)
        {
            if (YawMode == 5)
                ang.Y -= DesyncAmount;
            if (YawMode == 6)
                ang.Y += DesyncAmount;
        }

        private static void WriteModelAngles(Vector3 fake)
        {
            nint pawn = GameState.LocalPlayerPawn;
            if (pawn == 0) return;

            GameState.swed.WriteFloat(pawn + Offsets.m_lookYaw, fake.Y);
            GameState.swed.WriteFloat(pawn + Offsets.m_lookYawVel, 0f);

            GameState.swed.WriteFloat(pawn + Offsets.m_lookPitch, fake.X);
            GameState.swed.WriteFloat(pawn + Offsets.m_lookPitchVel, 0f);
        }

        private static void BreakMovement()
        {
            nint flags = GameState.LocalPlayerPawn + Offsets.m_fFlags;
            uint f = GameState.swed.ReadUInt(flags);
            GameState.swed.WriteUInt(flags, f ^ 1);
        }

        private static void RemoveDuckDelay()
        {
            GameState.swed.WriteFloat(GameState.LocalPlayerPawn + Offsets.m_flC4Blow, 1f);
        }

        private static void Normalize(ref Vector3 ang)
        {
            if (ang.X > 89) ang.X = 89;
            if (ang.X < -89) ang.X = -89;

            if (ang.Y > 180) ang.Y -= 360;
            if (ang.Y < -180) ang.Y += 360;

            ang.Z = 0;
        }
    }
}
