using System;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public class AntiAim : ThreadService
    {
        public static bool Enabled = false;
        public static bool Jitter = true;
        public static bool Spin = false;
        public static bool UsePitch = false;

        public static float JitterRange = 30f;
        public static float SpinSpeed = 5f;

        private float spinAngle = 0f;
        private bool jitterState = false;

        protected override void FrameAction()
        {
            if (!Enabled)
                return;

            var local = GameState.LocalPlayer;
            if (local == null || local.Health <= 0)
                return;

            if (local.IsAttacking)
                return; // don't AA while shooting

            RunAA(local);
        }

        private void RunAA(Data.Entity.Entity local)
        {
            // Base angle = current view angles
            Vector3 fake = local.ViewAngles;

            if (Jitter)
            {
                jitterState = !jitterState;
                fake.Y += jitterState ? JitterRange : -JitterRange;
            }

            if (Spin)
            {
                spinAngle += SpinSpeed;
                if (spinAngle > 180f) spinAngle -= 360f;
                fake.Y = spinAngle;
            }

            if (UsePitch)
            {
                fake.X = 89f; // legit fake pitch
            }
            else
            {
                fake.X = 0f; // standard
            }

            // Normalize
            Normalize(ref fake);

            // Write FAKE ANGLES ONLY to CSGOInput
            IntPtr input = GameState.swed.ReadPointer(GameState.client + Offsets.dwCSGOInput);
            if (input == IntPtr.Zero)
                return;

            IntPtr anglePtr = input + Offsets.dwViewAngles;
            GameState.swed.WriteVec(anglePtr, fake);

            // REAL ANGLES stay untouched → natural movement
        }

        private void Normalize(ref Vector3 ang)
        {
            if (ang.X > 89f) ang.X = 89f;
            if (ang.X < -89f) ang.X = -89f;

            while (ang.Y > 180f) ang.Y -= 360f;
            while (ang.Y < -180f) ang.Y += 360f;

            ang.Z = 0;
        }
    }
}
