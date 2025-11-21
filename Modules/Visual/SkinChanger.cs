using System;
using System.Collections.Generic;
using System.Numerics;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Visual
{
    public static class SkinChanger
    {
        public static bool Enabled = false;

        // selected loadout
        public static Dictionary<int, WeaponSkinData> WeaponSkins = new();
        public static KnifeType SelectedKnife = KnifeType.Karambit;
        public static int SelectedGlove = 0;
        public static int SelectedGlovePaint = 0;

        // internal
        private static bool pendingUpdate = false;

        public static void Run()
        {
            if (!Enabled)
                return;

            var lp = GameState.LocalPlayer;
            if (lp == null || lp.Health <= 0)
                return;

            ApplyWeapon(lp);
            ApplyKnife(lp);
            ApplyGloves(lp);

            if (pendingUpdate)
            {
                ForceUpdate();
                pendingUpdate = false;
            }
        }

        private static void ApplyWeapon(Entity lp)
        {
            IntPtr weaponServices = lp.PawnAddress + Offsets.m_pWeaponServices;
            if (weaponServices == 0)
                return;

            IntPtr hWeapon = GameState.swed.ReadPointer(weaponServices + Offsets.m_hActiveWeapon);
            if (hWeapon == 0)
                return;

            int index = (int)((hWeapon & 0x7FFF) - 1);
            IntPtr weapon = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList + index * 0x20);
            if (weapon == 0)
                return;

            int wpnID = GameState.swed.ReadInt(weapon + Offsets.m_iItemDefinitionIndex);

            if (!WeaponSkins.TryGetValue(wpnID, out var skin))
                return;

            ApplyFallback(weapon, skin);
        }

        private static void ApplyKnife(Entity lp)
        {
            IntPtr weaponServices = lp.PawnAddress + Offsets.m_pWeaponServices;
            if (weaponServices == 0)
                return;

            // read loadout: my_weapons[0..] until knife found
            for (int i = 0; i < 64; i++)
            {
                IntPtr hWeapon = GameState.swed.ReadPointer(weaponServices + Offsets.m_hMyWeapons + i * 0x8);
                if (hWeapon == 0)
                    continue;

                int index = (int)((hWeapon & 0x7FFF) - 1);
                IntPtr weapon = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList + index * 0x20);
                if (weapon == 0)
                    continue;

                int id = GameState.swed.ReadInt(weapon + Offsets.m_iItemDefinitionIndex);
                if (!IsKnife(id))
                    continue;

                int knifeID = (int)SelectedKnife;
                GameState.swed.WriteInt(weapon + Offsets.m_iItemDefinitionIndex, knifeID);

                ApplyFallback(weapon, new WeaponSkinData
                {
                    PaintKit = KnifePaints[SelectedKnife],
                    Seed = 0,
                    Wear = 0.0001f,
                    StatTrak = -1,
                    NameTag = ""
                });
            }
        }

        private static void ApplyGloves(Entity lp)
        {
            if (SelectedGlove == 0)
                return;

            // hands entity = local pawn + modelstate 
            IntPtr pawn = lp.PawnAddress;
            IntPtr gloveEnt = GameState.swed.ReadPointer(pawn + 0x150); // hand entity handle
            if (gloveEnt == 0)
                return;

            GameState.swed.WriteInt(gloveEnt + Offsets.m_iItemDefinitionIndex, SelectedGlove);
            GameState.swed.WriteInt(gloveEnt + 0x1BA, SelectedGlovePaint);
            GameState.swed.WriteFloat(gloveEnt + 0x1C0, 0.0001f);

            pendingUpdate = true;
        }

        private static void ApplyFallback(IntPtr weapon, WeaponSkinData skin)
        {
            GameState.swed.WriteInt(weapon + 0x1BA, skin.PaintKit);
            GameState.swed.WriteInt(weapon + 0x1BE, skin.Seed);
            GameState.swed.WriteFloat(weapon + 0x1C0, skin.Wear);
            GameState.swed.WriteInt(weapon + 0x1C4, skin.StatTrak);

            if (!string.IsNullOrEmpty(skin.NameTag))
                GameState.swed.WriteString(weapon + 0x1C8, skin.NameTag);

            pendingUpdate = true;
        }

        private static bool IsKnife(int id)
        {
            return id is 500 or 503 or 505 or 506 or 507 or 508 or 509 or 512 or 514 or 515 or 516;
        }

        private static void ForceUpdate()
        {
            IntPtr globals = GameState.swed.ReadPointer(GameState.client + Offsets.dwGlobalVars);
            if (globals != 0)
            {
                GameState.swed.WriteInt(globals + 0x14, -1);
            }
        }

        // SKIN DATABASE

        public enum KnifeType
        {
            Bayonet = 500,
            Bowie = 514,
            Butterfly = 515,
            Falchion = 512,
            Flip = 505,
            Gut = 506,
            Huntsman = 509,
            Karambit = 507,
            M9 = 508,
            Skeleton = 516
        }

        public static Dictionary<KnifeType, int> KnifePaints = new()
        {
            { KnifeType.Karambit, 38 },  // fade
            { KnifeType.M9, 415 },      // lore
            { KnifeType.Butterfly, 579 },
            { KnifeType.Skeleton, 1119 },
            { KnifeType.Bayonet, 44 },
        };

        public class WeaponSkinData
        {
            public int PaintKit = 0;
            public int Seed = 0;
            public float Wear = 0.0001f;
            public int StatTrak = -1;
            public string NameTag = "";
        }
    }
}
