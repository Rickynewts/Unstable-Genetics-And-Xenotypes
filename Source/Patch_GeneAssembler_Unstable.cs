using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UnstableGenetics
{
    ///虚拟包基因库
    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.GetGenepacks))]
    public static class Patch_GeneAssembler_GetGenepacks
    {
        private static bool Prefix(Building_GeneAssembler __instance, bool includePowered, bool includeUnpowered, ref List<Genepack> __result)
        {
            if (__instance is Building_UnstableGeneassembler assembler)
            {
                __result = includePowered ? assembler.GetVirtualGenepacks() : new List<Genepack>();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.Start))]
    public static class Patch_GeneAssembler_Start
    {
        private static void Prefix(Building_GeneAssembler __instance, ref List<Genepack> packs, ref int architesRequired)
        {
            if (!(__instance is Building_UnstableGeneassembler assembler))
            {
                return;
            }

            architesRequired = 0;

            List<Genepack> merged = new List<Genepack>();
            if (packs != null)
            {
                merged.AddRange(packs);
            }
            int selectedGeneCount = 0;
            foreach (Genepack pack in merged)
            {
                if (pack?.GeneSet != null)
                {
                    selectedGeneCount += pack.GeneSet.GenesListForReading.Count;
                }
            }
            int unstableCount = Building_UnstableGeneassembler.UnstableCountFor(selectedGeneCount);
            List<Genepack> unstablePacks;
            if (!assembler.presetUnstableGenes.NullOrEmpty())
            {
                unstablePacks = assembler.GetPacksForGenes(assembler.presetUnstableGenes);
            }
            else
            {
                assembler.RefreshRandomUnstable(selectedGeneCount);
                unstablePacks = assembler.GetPacksForGenes(assembler.randomUnstableGenes);
            }
            merged.AddRange(unstablePacks);
            packs = merged;

            assembler.pendingGenes = new List<GeneDef>();
            foreach (Genepack pack in merged)
            {
                if (pack?.GeneSet != null)
                {
                    assembler.pendingGenes.AddRange(pack.GeneSet.GenesListForReading);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.MaxComplexity))]
    public static class Patch_GeneAssembler_MaxComplexity
    {
        private static bool Prefix(Building_GeneAssembler __instance, ref int __result)
        {
            if (__instance is Building_UnstableGeneassembler)
            {
                __result = Building_UnstableGeneassembler.ComplexityLimit;
                return false;
            }
            return true;
        }
    }
    /// 原版每180tick检查
    [HarmonyPatch]
    public static class Patch_GeneAssembler_CheckContainers
    {
        private static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Building_GeneAssembler), "CheckAllContainersValid");
        }

        private static bool Prefix(Building_GeneAssembler __instance)
        {
            return !(__instance is Building_UnstableGeneassembler);
        }
    }
}