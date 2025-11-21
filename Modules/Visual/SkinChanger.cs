using System;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visuals
{
    public class SkinChanger : ThreadService
    {
        public override string Name => "SkinChanger";

        public static bool Enabled = true; // always enabled

        protected override void FrameAction()
        {
            if (!Enabled)
                return;

            var lp = GameState.LocalPlayer;
            if (lp == null || lp.PawnAddress == IntPtr.Zero)
                return;

            RenameAllWeapons(lp);
        }

        private void RenameAllWeapons(Entity lp)
        {
            var swed = GameState.swed;
            IntPtr pawn = lp.PawnAddress;
            if (pawn == IntPtr.Zero)
                return;

            // weapon services
            IntPtr weaponServices = swed.ReadPointer(pawn + Offsets.m_pWeaponServices);
            if (weaponServices == IntPtr.Zero)
                return;

            // handle array
            IntPtr myWeapons = weaponServices + Offsets.m_hMyWeapons;

            for (int i = 0; i < 64; i++)
            {
                int handle = swed.ReadInt(myWeapons + i * 4);
                if (handle == 0)
                    continue;

                IntPtr weapon = GetEntityByHandle(handle);
                if (weapon == IntPtr.Zero)
                    continue;

                // attribute manager
                IntPtr attrManager = weapon + Offsets.m_AttributeManager;
                IntPtr namePtr = attrManager + Offsets.m_szCustomName;

                // write name
                swed.WriteString(namePtr, "ygy.win\0");
            }
        }

        private IntPtr GetEntityByHandle(int handle)
        {
            int index = handle & 0x7FFF;
            return GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList + (index * 8));
        }
    }
}