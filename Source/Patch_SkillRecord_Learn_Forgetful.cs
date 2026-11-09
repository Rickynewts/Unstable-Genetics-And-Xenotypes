using HarmonyLib;
using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 健忘基因
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
    public static class Patch_SkillRecord_Learn_Forgetful
    {
        private const float DefaultMultiplier = 1.5f;

        // 这个 patch 只服务于 UG_Forgetful 一个基因，
        // 用单值缓存就够了，不必在每次负经验时做字典查找。
        private static GeneDef cachedGeneDef;
        private static float cachedMultiplier = DefaultMultiplier;

        [HarmonyPrefix]
        public static void Prefix(SkillRecord __instance, ref float xp)
        {
            // Learn 是热路径（工作、战斗都在加经验），正经验必须最先退出。
            if (xp >= 0f)
            {
                return;
            }

            GeneDef forgetfulDef = UG_DefOf.UG_Forgetful;
            if (forgetfulDef == null)
            {
                return;
            }

            Pawn pawn = __instance.Pawn;
            if (pawn?.genes == null || !pawn.genes.HasActiveGene(forgetfulDef))
            {
                return;
            }

            xp *= Multiplier(forgetfulDef);
        }

        private static float Multiplier(GeneDef geneDef)
        {
            if (cachedGeneDef != geneDef)
            {
                ModExtension_Forgetful ext = geneDef.GetModExtension<ModExtension_Forgetful>();
                cachedMultiplier = (ext != null && ext.forgetMultiplier > 0f) ? ext.forgetMultiplier : DefaultMultiplier;
                cachedGeneDef = geneDef;
            }
            return cachedMultiplier;
        }
    }
}
