using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UnstableGenetics
{
    ///肾衰竭
    public class Gene_KidneyFailure : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            ApplyOrganDecay();
        }

        private void ApplyOrganDecay()
        {
            if (pawn == null || pawn.Dead)
            {
                return;
            }

            List<BodyPartRecord> candidates = new List<BodyPartRecord>();
            foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts(tag: BodyPartTagDefOf.BloodFiltrationKidney))
            {
                //人造器官跳过
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                {
                    continue;
                }
                //这一侧已经在衰竭就跳过
                if (pawn.health.hediffSet.HasHediff(HediffDefOf.OrganDecay, part))
                {
                    continue;
                }
                candidates.Add(part);
            }

            if (candidates.Count == 0)
            {
                return;
            }

            //选择随机一侧肾脏
            pawn.health.AddHediff(HediffDefOf.OrganDecay, candidates.RandomElement());
        }
    }
}