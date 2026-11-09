using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 恐水症的心情效果：
    /// 阶段 0 = 正在下雨，阶段 1 = 站在水里（更严重）。
    public class ThoughtWorker_WaterTerror : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.genes == null || !p.genes.HasActiveGene(UG_DefOf.UG_WaterTerror))
            {
                return ThoughtState.Inactive;
            }
            if (!p.Spawned || p.Map == null)
            {
                return ThoughtState.Inactive;
            }

            if (p.Position.GetTerrain(p.Map).IsWater)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (p.Map.weatherManager.RainRate > 0.01f)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
}