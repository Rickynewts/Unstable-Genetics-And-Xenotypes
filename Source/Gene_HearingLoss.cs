using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 听力损失：携带者双耳的听力细胞退化，每只耳朵的部件效率降低 50%（机制同白内障）。
    public class Gene_HearingLoss : Gene
    {
        /// <summary>
        /// HediffDef / BodyPartDef 只查一次，避免每次添加基因都走一遍 DefDatabase。
        /// </summary>
        private static HediffDef cachedHearingLossDef;
        private static BodyPartDef cachedEarDef;

        private static HediffDef HearingLossDef
        {
            get
            {
                if (cachedHearingLossDef == null)
                {
                    cachedHearingLossDef = DefDatabase<HediffDef>.GetNamedSilentFail("HearingLoss");
                }
                return cachedHearingLossDef;
            }
        }

        private static BodyPartDef EarDef
        {
            get
            {
                if (cachedEarDef == null)
                {
                    cachedEarDef = DefDatabase<BodyPartDef>.GetNamedSilentFail("Ear");
                }
                return cachedEarDef;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            ApplyHearingLoss();
        }

        private void ApplyHearingLoss()
        {
            if (pawn == null || pawn.Dead || pawn.health?.hediffSet == null)
            {
                return;
            }

            HediffDef hearingLossDef = HearingLossDef;
            if (hearingLossDef == null)
            {
                return;
            }

            BodyPartDef earDef = EarDef;
            if (earDef == null)
            {
                return;
            }

            foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (part.def != earDef)
                {
                    continue;
                }

                // 仿生耳/植入体不会退化
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                {
                    continue;
                }

                if (pawn.health.hediffSet.HasHediff(hearingLossDef, part))
                {
                    continue;
                }

                pawn.health.AddHediff(hearingLossDef, part);
            }
        }
    }
}