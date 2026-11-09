using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace UnstableGenetics
{
    ///载入预设
    [HarmonyPatch(typeof(GeneCreationDialogBase), "DoBottomButtons")]
    public static class Patch_Dialog_LoadPresetButton
    {
        private static readonly AccessTools.FieldRef<GeneCreationDialogBase, string> NameRef =
            AccessTools.FieldRefAccess<GeneCreationDialogBase, string>("xenotypeName");

        private static readonly AccessTools.FieldRef<GeneCreationDialogBase, bool> NameLockedRef =
            AccessTools.FieldRefAccess<GeneCreationDialogBase, bool>("xenotypeNameLocked");

        internal static readonly AccessTools.FieldRef<Dialog_CreateXenogerm, Building_GeneAssembler> AssemblerRef =
            AccessTools.FieldRefAccess<Dialog_CreateXenogerm, Building_GeneAssembler>("geneAssembler");

        private static readonly AccessTools.FieldRef<Dialog_CreateXenogerm, List<Genepack>> SelectedRef =
            AccessTools.FieldRefAccess<Dialog_CreateXenogerm, List<Genepack>>("selectedGenepacks");

        private static readonly AccessTools.FieldRef<Dialog_CreateXenogerm, List<Genepack>> LibraryRef =
            AccessTools.FieldRefAccess<Dialog_CreateXenogerm, List<Genepack>>("libraryGenepacks");

        private static void Postfix(GeneCreationDialogBase __instance, Rect rect)
        {
            if (!(__instance is Dialog_CreateXenogerm dialog))
            {
                return;
            }
            if (!(AssemblerRef(dialog) is Building_UnstableGeneassembler))
            {
                return;
            }

            //底部按钮栏
            if (!Widgets.ButtonText(new Rect(rect.x + 158f, rect.y, 150f, 38f), "UG_LoadPreset".Translate()))
            {
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (XenotypeDef xenotype in DefDatabase<XenotypeDef>.AllDefsListForReading
                .Where(x => x.modContentPack != null
                    && string.Equals(x.modContentPack.PackageId, "yushiren.unstablegenetics", System.StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.displayPriority))
            {
                XenotypeDef captured = xenotype;
                options.Add(new FloatMenuOption(
                    captured.LabelCap,
                    () => Apply(dialog, captured),
                    captured.Icon,
                    XenotypeDef.IconColor,
                    MenuOptionPriority.Default,
                    r => TooltipHandler.TipRegion(r, captured.descriptionShort ?? captured.description)));
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("UG_NoPresetAvailable".Translate(), null));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void Apply(Dialog_CreateXenogerm dialog, XenotypeDef xenotype)
        {
            Building_UnstableGeneassembler assembler = AssemblerRef(dialog) as Building_UnstableGeneassembler;
            List<Genepack> selected = SelectedRef(dialog);
            List<Genepack> library = LibraryRef(dialog);

            selected.Clear();
            if (assembler != null)
            {
                assembler.presetUnstableGenes = new List<GeneDef>();
            }

            //名称与预设同步
            NameRef(dialog) = xenotype.label;
            NameLockedRef(dialog) = true;

            foreach (GeneDef gene in xenotype.genes)
            {
                if (Building_UnstableGeneassembler.UnstableGeneDefNameSet.Contains(gene.defName))
                {
                    if (assembler != null)
                    {
                        assembler.presetUnstableGenes.Add(gene);
                    }
                    continue;
                }

                List<Genepack> pool = (assembler != null ? assembler.GetVirtualGenepacks() : library);
                Genepack pack = pool.FirstOrDefault(p => p.GeneSet != null && p.GeneSet.GenesListForReading.Contains(gene));
                if (pack != null && !selected.Contains(pack))
                {
                    selected.Add(pack);
                }
            }

            AccessTools.Method(typeof(GeneCreationDialogBase), "OnGenesChanged")?.Invoke(dialog, null);
        }
    }
    ///手动选/取消任何基因时，清空预设
    [HarmonyPatch(typeof(Dialog_CreateXenogerm), "DrawGenepack")]
    public static class Patch_Dialog_ClearPresetOnPick
    {
        private static void Postfix(Dialog_CreateXenogerm __instance, bool __result)
        {
            if (!__result)
            {
                return;
            }
            if (Patch_Dialog_LoadPresetButton.AssemblerRef(__instance) is Building_UnstableGeneassembler assembler)
            {
                assembler.presetUnstableGenes = null;
            }
        }
    }
    /// 关闭窗口时清空预设
    [HarmonyPatch(typeof(Window), "PostClose")]
    public static class Patch_Dialog_ClearPresetOnClose
    {
        private static void Postfix(Window __instance)
        {
            if (!(__instance is Dialog_CreateXenogerm dialog))
            {
                return;
            }
            if (Patch_Dialog_LoadPresetButton.AssemblerRef(dialog) is Building_UnstableGeneassembler assembler)
            {
                assembler.presetUnstableGenes = null;
            }
        }
    }

    ///统计复杂度/代谢率
    [HarmonyPatch(typeof(Dialog_CreateXenogerm), "get_SelectedGenes")]
    public static class Patch_Dialog_SelectedGenes
    {
        private static void Postfix(Dialog_CreateXenogerm __instance, ref List<GeneDef> __result)
        {
            if (!(Patch_Dialog_LoadPresetButton.AssemblerRef(__instance) is Building_UnstableGeneassembler assembler))
            {
                return;
            }
            List<GeneDef> extra = !assembler.presetUnstableGenes.NullOrEmpty()
                ? assembler.presetUnstableGenes
                : assembler.randomUnstableGenes;
            if (extra.NullOrEmpty())
            {
                return;
            }

            // 原版 SelectedGenes 返回的是内部复用的临时列表（每次 getter 先 Clear 再重建），
            // 所以直接往它上面追加即可。这样这个 getter 每帧被调用时也不会再分配新列表。
            if (__result == null)
            {
                __result = new List<GeneDef>();
            }
            __result.AddRange(extra);
        }
    }
}