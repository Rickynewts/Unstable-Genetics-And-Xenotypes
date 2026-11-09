using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 恐水症。
    public class Gene_WaterTerror : Gene
    {
        private const int CheckInterval = 250;

        public override void TickInterval(int delta)
        {
            // 保留基因基类的逻辑（原版的基因精神崩溃判定等）。
            base.TickInterval(delta);

            if (pawn == null || pawn.Dead || pawn.Map == null || !pawn.Spawned || pawn.health?.hediffSet == null)
            {
                return;
            }

            // 每 250 tick 才检查一次地形，避免每 tick 都做区域查询。
            if (!pawn.IsHashIntervalTick(CheckInterval, delta))
            {
                return;
            }

            bool inWater = pawn.Position.GetTerrain(pawn.Map).IsWater;
            Hediff fear = pawn.health.hediffSet.GetFirstHediffOfDef(UG_DefOf.UG_WaterTerrorFear);

            if (inWater)
            {
                if (fear == null)
                {
                    pawn.health.AddHediff(UG_DefOf.UG_WaterTerrorFear);
                }
            }
            else if (fear != null)
            {
                pawn.health.RemoveHediff(fear);
            }
        }
    }
}
