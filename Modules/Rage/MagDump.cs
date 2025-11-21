using System;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class DoubleTap
    {
        public static bool Enabled       = false;
        public static int    TickShift   = 6;     // how many ticks you shift back

        private static bool   ready      = true;
        private static long   lastFire   = 0;
        private const double  cooldown   = 0.4;   // seconds

        public static void Run()
        {
            if (!Enabled)
                return;

            var lp = GameState.LocalPlayer;
            var pawn = GameState.LocalPlayerPawn;
            if (lp == null || pawn == 0 || lp.Health <= 0)
                return;

            bool attacking = GameState.swed.ReadBool(GameState.client + Offsets.attack);
            if (!attacking)
                return;

            if (!ready && TimeSince(lastFire) < cooldown)
                return;

            // tickbase shift logic
            int currentTick = GameState.swed.ReadInt(pawn + Offsets.m_nTickBase);
            GameState.swed.WriteInt(pawn + Offsets.m_nTickBase, currentTick - TickShift);

            // force two shots
            ForceAttack(1);
            ForceAttack(0);
            ForceAttack(1);
            ForceAttack(0);

            lastFire = Environment.TickCount64;
            ready    = false;
        }

        private static void ForceAttack(int state)
        {
            GameState.swed.WriteInt(GameState.client + Offsets.attack, state);
        }

        private static double TimeSince(long t)
        {
            return (Environment.TickCount64 - t) / 1000.0;
        }
    }
}