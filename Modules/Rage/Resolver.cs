using System;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public static class Resolver
    {
        public static bool Enabled = true;

        private struct ResolverRecord
        {
            public float LastYaw;
            public float LastDelta;
            public float FakeSide;
        }

        private static readonly Dictionary<IntPtr, ResolverRecord> records = new();

        public static Vector3 Resolve(Entity e)
        {
            if (!Enabled)
                return e.ViewAngles;

            if (e == null || e.Health <= 0)
                return e.ViewAngles;

            if (!records.ContainsKey(e.PawnAddress))
                records[e.PawnAddress] = new ResolverRecord();

            var rec = records[e.PawnAddress];

            float currentYaw = e.ViewAngles.Y;
            float delta = NormalizeAngle(currentYaw - rec.LastYaw);

            // detect fake side by yaw behavior
            float fakeSide = rec.FakeSide;
            if (Math.Abs(delta) > 15f)
                fakeSide = MathF.Sign(delta);

            Vector3 resolved = e.ViewAngles;
            resolved.Y += fakeSide * 35f; // classic desync correction

            rec.LastDelta = delta;
            rec.LastYaw = currentYaw;
            rec.FakeSide = fakeSide;

            records[e.PawnAddress] = rec;

            Normalize(ref resolved);
            return resolved;
        }

        private static float NormalizeAngle(float a)
        {
            while (a > 180f) a -= 360f;
            while (a < -180f) a += 360f;
            return a;
        }

        private static void Normalize(ref Vector3 ang)
        {
            if (ang.X > 89) ang.X = 89;
            if (ang.X < -89) ang.X = -89;

            while (ang.Y > 180) ang.Y -= 360;
            while (ang.Y < -180) ang.Y += 360;
            ang.Z = 0;
        }
    }
}