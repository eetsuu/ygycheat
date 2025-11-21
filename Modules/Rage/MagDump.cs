using System;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public class DoubleTap : ThreadService
    {
        public static bool Enabled = false;
        public static bool RequireKey = false;
        public static int Key = (int)ConsoleKey.X; // You can change in GUI later

        private readonly EntityManager entMgr = new();

        protected override void FrameAction()
        {
            if (!Enabled)
                return;

            if (RequireKey && (GetAsyncKeyState(Key) & 0x8000) == 0)
                return;

            if (GameState.LocalPlayer == null || GameState.LocalPlayer.Health <= 0)
                return;

            RunMagDump();
        }

        private void RunMagDump()
        {
            // active weapon
            IntPtr weaponServices = GameState.swed.ReadPointer(GameState.LocalPlayerPawn + Offsets.m_pWeaponServices);
            if (weaponServices == IntPtr.Zero)
                return;

            IntPtr weaponHandle = GameState.swed.ReadPointer(weaponServices + Offsets.m_hActiveWeapon);
            if (weaponHandle == IntPtr.Zero)
                return;

            IntPtr weapon = GameState.swed.ReadPointer(weaponHandle + Offsets.m_Item);
            if (weapon == IntPtr.Zero)
                return;

            // next primary attack block
            float nextAttack = GameState.swed.ReadFloat(weapon + 0x3C); // Usually close to "m_flNextPrimaryAttack"
            if (nextAttack > 0f)
            {
                // force cooldown to 0
                GameState.swed.WriteFloat(weapon + 0x3C, 0f);
            }

            // global attack cooldown
            IntPtr attackPtr = GameState.client + Offsets.attack;
            GameState.swed.WriteInt(attackPtr, 6);    // press attack
            Thread.Sleep(1);
            GameState.swed.WriteInt(attackPtr, 4);    // release attack
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);
    }
}
