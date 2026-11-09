using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 疤痕体质
    [HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.PreFinalizeInjury))]
    public static class Patch_HypertrophicScar
    {
        private const float ScarChanceMultiplier = 3f;

        private static bool Prefix(HediffComp_GetsPermanent __instance)
        {
            Pawn pawn = __instance.Pawn;
            if (pawn == null || pawn.genes == null || !pawn.genes.HasActiveGene(UG_DefOf.UG_HypertrophicScar))
            {
                return true; 
            }

            Hediff parent = __instance.parent;
            if (parent == null || parent.Part == null)
            {
                return true;
            }
            if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part))
            {
                return true;
            }
            if (ModsConfig.BiotechActive && HasGenePreventingPermanentWounds(pawn))
            {
                return true;
            }

            float chance = 0.02f
                * parent.Part.def.permanentInjuryChanceFactor
                * __instance.Props.becomePermanentChanceFactor;
            if (!parent.Part.def.delicate)
            {
                chance *= HealthTuning.BecomePermanentChanceFactorBySeverityCurve.Evaluate(parent.Severity);
            }
            chance *= ScarChanceMultiplier;

            if (Rand.Chance(chance))
            {
                if (parent.Part.def.delicate)
                {
                    __instance.IsPermanent = true;
                }
                else
                {
                    __instance.permanentDamageThreshold = Rand.Range(1f, parent.Severity / 2f);
                }
            }
            return false;
        }

        /// <summary>
        /// 受伤判定会频繁触发，这里用显式循环代替 LINQ Any，
        /// 避免每次受伤都分配闭包和枚举器。
        /// </summary>
        private static bool HasGenePreventingPermanentWounds(Pawn pawn)
        {
            List<Gene> genes = pawn.genes.GenesListForReading;
            for (int i = 0; i < genes.Count; i++)
            {
                Gene gene = genes[i];
                if (gene != null && gene.def != null && gene.def.preventPermanentWounds)
                {
                    return true;
                }
            }
            return false;
        }
    }
}