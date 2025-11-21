using Titled_Gui.Data.Game;

public static class ThirdPerson
{
    public static bool Enabled = false;

    public static void Run()
    {
        if (!Enabled)
            return;

        // Get camera services pointer
        nint cam = GameState.swed.ReadPointer(GameState.LocalPlayerPawn + Offsets.m_pCameraServices);
        if (cam == 0)
            return;

        GameState.swed.WriteInt(cam + Offsets.m_iCameraMode, 1); // enable TP
    }

    public static void Disable()
    {
        nint cam = GameState.swed.ReadPointer(GameState.LocalPlayerPawn + Offsets.m_pCameraServices);
        if (cam == 0)
            return;

        GameState.swed.WriteInt(cam + Offsets.m_iCameraMode, 0);
    }
}