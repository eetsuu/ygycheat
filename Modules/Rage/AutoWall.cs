using System;
using System.Numerics;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Rage
{
    public static class AutoWall
    {
        public static bool Enabled = false;
        public static float MinDamage = 20f;

        public static bool CanHit(Entity target)
        {
            Vector3 src = GameState.LocalPlayer.Head;
            Vector3 dst = RageManager.GetBone(target);

            Vector3 dir = Vector3.Normalize(dst - src);
            float step = 12f;
            float currentDamage = 100f;
            Vector3 cur = src;

            for (float d = 0; d < 4096; d += step)
            {
                cur += dir * step;

                bool hitsPlayer =
                    Vector3.Distance(cur, dst) < 10f;

                if (hitsPlayer)
                    return currentDamage >= MinDamage;

                currentDamage -= 5f;
                if (currentDamage <= 0)
                    return false;
            }

            return false;
        }
    }
}