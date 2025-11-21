using System;
using System.Numerics;
using ImGuiNET;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class AAPreview
    {
        private static float anim = 0f;

        public static void DrawPreview()
        {
            Vector2 size = new(180, 180);
            Vector2 pos = ImGui.GetCursorScreenPos();

            var dl = ImGui.GetWindowDrawList();

            Vector2 center = pos + size / 2;

            // background circle
            dl.AddCircleFilled(center, size.X / 2, ImGui.ColorConvertFloat4ToU32(new(0.08f, 0.0f, 0.12f, 1f)));

            // spin anim
            anim += 1.5f;
            if (anim > 360) anim -= 360;

            // draw real angle
            float real = AntiAim.RealAngle.Y;
            float fake = AntiAim.FakeAngle.Y;

            DrawArrow(dl, center, real, 60, new Vector4(0.9f, 0.3f, 1f, 1f));  // real (pink)
            DrawArrow(dl, center, fake, 40, new Vector4(0.3f, 0.8f, 1f, 1f));  // fake (blue)

            // outline
            dl.AddCircle(center, size.X / 2, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.15f)), 64, 2f);

            ImGui.Dummy(size);
        }

        private static void DrawArrow(ImDrawListPtr dl, Vector2 center, float yawDeg, float length, Vector4 color)
        {
            float yawRad = (yawDeg - 90f) * (MathF.PI / 180f);

            Vector2 end = new(
                center.X + MathF.Cos(yawRad) * length,
                center.Y + MathF.Sin(yawRad) * length
            );

            dl.AddLine(center, end, ImGui.ColorConvertFloat4ToU32(color), 3f);

            Vector2 left = RotateAround(end, center, 150f);
            Vector2 right = RotateAround(end, center, -150f);

            dl.AddLine(end, left, ImGui.ColorConvertFloat4ToU32(color), 2f);
            dl.AddLine(end, right, ImGui.ColorConvertFloat4ToU32(color), 2f);
        }

        private static Vector2 RotateAround(Vector2 point, Vector2 origin, float degrees)
        {
            float rad = degrees * (MathF.PI / 180f);
            float s = MathF.Sin(rad);
            float c = MathF.Cos(rad);

            Vector2 p = point - origin;

            float x = p.X * c - p.Y * s;
            float y = p.X * s + p.Y * c;

            return new Vector2(x, y) + origin;
        }
    }
}
