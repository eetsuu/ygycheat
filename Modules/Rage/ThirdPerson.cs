using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class ThirdPersonController
    {
        public static bool Enabled = false;
        private static int tpVar = 0;

        public static void Run()
        {
            nint lp = GameState.LocalPlayerPawn;
            if (lp == 0) return;

            tpVar = Enabled ? 1 : 0;
            GameState.swed.WriteInt(lp + 0x16C, tpVar); // m_thirdPersonShoulder
        }
    }
}