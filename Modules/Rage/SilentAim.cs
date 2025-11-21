using System;
using System.Numerics;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Rage
{
    public static class SilentAim
    {
        public static bool Enabled = false;
        public static float FOV = 15f;

        private static bool lastAttack = false;

        public static void Run()
        {
            if (!Enabled) return;

            bool attack = GameState.swed.ReadBool(GameState.client, Offsets.attack);

            if (attack && !lastAttack)
            {
                Entity? target = RageManager.FindTarget(FOV);
                if (target != null)
                {
                    Vector3 aim = AimbotMath.CalcAngle(GameState.LocalPlayer.Head, RageManager.GetBone(target));
                    aim = AimbotMath.Normalize(aim);

                    GameState.swed.WriteVec(GameState.client, Offsets.dwViewAngles, aim);
                }
            }

            lastAttack = attack;
        }
    }
}