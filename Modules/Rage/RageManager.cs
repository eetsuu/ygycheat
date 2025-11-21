using System;
using System.Linq;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class RageManager
    {
        public static bool RageEnabled = false;

        public static int TargetBone = 8; // head by default
        public static float MinDamage = 20f;

        public static Entity? CurrentTarget { get; private set; }

        private static readonly EntityManager entMgr = new();

        public static void UpdateTarget()
        {
            if (!RageEnabled)
            {
                CurrentTarget = null;
                return;
            }

            var local = GameState.LocalPlayer;
            if (local == null || local.Health <= 0)
            {
                CurrentTarget = null;
                return;
            }

            var ents = entMgr.GetEntities();
            if (ents == null || ents.Count == 0)
            {
                CurrentTarget = null;
                return;
            }

            // Filter valid targets
            var valid = ents.Where(e =>
                e != null &&
                e.Health > 0 &&
                e.LifeState == 256 &&
                e.Team != local.Team &&
                e.Bones != null &&
                e.Bones.Count > TargetBone);

            if (!valid.Any())
            {
                CurrentTarget = null;
                return;
            }

            // Sort by distance or FOV
            CurrentTarget = valid.OrderBy(e => Vector3.Distance(local.Position, e.Position)).FirstOrDefault();
        }

        public static bool CanHitTarget()
        {
            if (CurrentTarget == null) 
                return false;

            if (!AutoWall.Enabled)
                return true;

            float damage = AutoWall.CalculateDamage(GameState.LocalPlayer!, CurrentTarget, TargetBone);
            return damage >= MinDamage;
        }

        public static Vector3 GetAimPoint()
        {
            if (CurrentTarget == null)
                return Vector3.Zero;

            return CurrentTarget.Bones![TargetBone];
        }
    }
}
