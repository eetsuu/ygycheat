using System;
using System.Collections.Generic;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public class AutoWall : ThreadService
    {
        public static bool Enabled = false;

        private readonly EntityManager entMgr = new();

        // Simplified weapon penetration table — can be expanded later
        private readonly Dictionary<string, float> WeaponPenetration = new()
        {
            { "ak47", 1.25f },
            { "m4a4", 1.10f },
            { "m4a1_silencer", 1.05f },
            { "awp", 2.0f },
            { "ssg08", 1.5f },
            { "scar20", 1.9f },
            { "g3sg1", 1.8f },
            { "deagle", 1.4f },
            { "revolver", 1.5f }
        };

        protected override void FrameAction()
        {
            if (!Enabled)
                return;

            if (GameState.LocalPlayer == null || GameState.LocalPlayer.Health <= 0)
                return;

            RunAW();
        }

        private void RunAW()
        {
            // Nothing is actively done here, AW is called by SilentAim or Aimbot.
            // This thread just stays alive for future expansion.
        }

        public float CalculateDamage(Entity local, Entity enemy, int bone)
        {
            if (enemy == null || enemy.Bones == null || enemy.Bones.Count <= bone)
                return 0f;

            Vector3 start = local.Position + local.View;
            Vector3 end = enemy.Bones[bone];

            // Distance-based falloff
            float distance = Vector3.Distance(start, end);

            float damage = 100f;
            damage -= distance * 0.015f;

            // Material wall thickness — basic external autowall
            float thickness = CalculateWallThickness(start, end);
            if (thickness < 0)
                return 0f;

            float penetrates = GetWeaponPenetration(local.CurrentWeaponName);

            damage -= thickness * (5f / penetrates);

            if (damage < 1f)
                damage = 0f;

            return damage;
        }

        private float GetWeaponPenetration(string? weapon)
        {
            if (weapon == null)
                return 1f;

            string clean = weapon.ToLower().Replace("weapon_", "");

            if (WeaponPenetration.TryGetValue(clean, out float pen))
                return pen;

            return 1.0f; // default
        }

        private float CalculateWallThickness(Vector3 start, Vector3 end)
        {
            // External cheats cannot raycast walls, so we simulate thickness
            // by checking world distance and subtracting visible delta.

            Vector3 dir = Vector3.Normalize(end - start);
            float maxRange = Vector3.Distance(start, end);

            float step = 4f; // resolution
            float traveled = 0f;
            int hits = 0;

            Vector3 pos = start;

            while (traveled < maxRange)
            {
                pos += dir * step;
                traveled += step;

                bool insideWall = IsPointInsideSolid(pos);
                if (insideWall)
                    hits++;
            }

            // convert hit-count to approximate wall thickness
            return hits * step;
        }

        private bool IsPointInsideSolid(Vector3 point)
        {
            // We approximate solidity using visibility state:
            // If point is not spotted by local player, treat as "solid"
            try
            {
                // Read world flags (fflag) — fflag 0x1 = on ground, 0x100 = solid?
                // Your repo has: m_fFlags = 0x3EC (stored in Offsets)
                uint flags = (uint)GameState.swed.ReadInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
                return (flags & 0x100) != 0;
            }
            catch
            {
                return false;
            }
        }

        public bool CanHit(Entity local, Entity enemy, int bone, float minDamage)
        {
            float dmg = CalculateDamage(local, enemy, bone);
            return dmg >= minDamage;
        }
    }
}
