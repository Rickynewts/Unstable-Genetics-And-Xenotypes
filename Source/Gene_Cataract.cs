using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 白内障
    public class Gene_Cataract : Gene
    {
        /// <summary>
        /// 原版白内障 HediffDef 只查一次；用 GetNamedSilentFail 避免 Def 缺失时抛异常。
        /// </summary>
        private static HediffDef cachedCataractDef;

        private static HediffDef CataractDef
        {
            get
            {
                if (cachedCataractDef == null)
                {
                    cachedCataractDef = DefDatabase<HediffDef>.GetNamedSilentFail("Cataract");
                }
                return cachedCataractDef;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            ApplyCataract();
        }

        private void ApplyCataract()
        {
            if (pawn == null || pawn.Dead || pawn.health?.hediffSet == null)
            {
                return;
            }

            //获取原版白内障HediffDef
            HediffDef cataractDef = CataractDef;
            if (cataractDef == null)
            {
                return;
            }

            //遍历所有未缺失的身体部位
            foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts())
            {
                //只针对原版定义的Eye部位
                if (part.def != BodyPartDefOf.Eye)
                {
                    continue;
                }

                //人造跳过
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                {
                    continue;
                }

                //已有跳过
                if (pawn.health.hediffSet.HasHediff(cataractDef, part))
                {
                    continue;
                }

                //添加白内障
                pawn.health.AddHediff(cataractDef, part);
            }
        }
    }
}