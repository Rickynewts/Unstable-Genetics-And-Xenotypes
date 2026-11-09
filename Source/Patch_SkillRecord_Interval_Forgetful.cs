using HarmonyLib;
using RimWorld;
using Verse;

namespace UnstableGenetics
{
    /// 健忘
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Interval))]
    public static class Patch_SkillRecord_Interval_Forgetful
    {
        [HarmonyPostfix]
        public static void Postfix(SkillRecord __instance, bool __runOriginal)
        {
            // 原方法被跳过
            if (!__runOriginal)
            {
                return;
            }

            // 原版 Interval 每 200 tick 对每个技能跑一次，
            // 先做最便宜的判断，尽量让大多数调用直接退出。
            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            // 10 级及以上原版已经处理过，这里只补低等级
            if (__instance.levelInt >= 10)
            {
                return;
            }

            Pawn pawn = __instance.Pawn;
            if (pawn?.genes == null || !pawn.genes.HasActiveGene(UG_DefOf.UG_Forgetful))
            {
                return;
            }

            // 与原版 Interval 保持一致：完美记忆（Anomaly）免疫遗忘
            if (ModsConfig.AnomalyActive && TraitDefOf.PerfectMemory != null
                && pawn.story?.traits != null
                && pawn.story.traits.HasTrait(TraitDefOf.PerfectMemory))
            {
                return;
            }

            // 良好记忆让遗忘减半，同样与原版一致
            float factor = (pawn.story?.traits != null && pawn.story.traits.HasTrait(TraitDefOf.GreatMemory)) ? 0.5f : 1f;
            __instance.Learn(-0.1f * factor);
        }
    }
}
