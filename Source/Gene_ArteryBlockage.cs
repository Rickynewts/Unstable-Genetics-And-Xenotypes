using RimWorld;
using Verse;

namespace UnstableGenetics
{
    ///动脉阻塞。
    public class Gene_ArteryBlockage : Gene
    {
        /// <summary>
        /// 原版 HediffDef 只查一次，避免每次添加基因都做一次字符串查找。
        /// 用 GetNamedSilentFail 而不是 Named，避免原版 Def 不存在时直接抛异常。
        /// </summary>
        private static HediffDef cachedBlockageDef;

        private static HediffDef BlockageDef
        {
            get
            {
                if (cachedBlockageDef == null)
                {
                    cachedBlockageDef = DefDatabase<HediffDef>.GetNamedSilentFail("HeartArteryBlockage");
                }
                return cachedBlockageDef;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            ApplyArteryBlockage();
        }

        private void ApplyArteryBlockage()
        {
            if (pawn == null || pawn.Dead || pawn.health?.hediffSet == null)
            {
                return;
            }

            HediffDef blockageDef = BlockageDef;
            if (blockageDef == null)
            {
                return;
            }

            foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (part.def != BodyPartDefOf.Heart)//仅限心脏
                {
                    continue;
                }

                //
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))//仿生部件跳过
                {
                    continue;
                }

                if (pawn.health.hediffSet.HasHediff(blockageDef, part))//已有就不重复加
                {
                    continue;
                }

                pawn.health.AddHediff(blockageDef, part);//加阻塞
            }
        }
    }
}