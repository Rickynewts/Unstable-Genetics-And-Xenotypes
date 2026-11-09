using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace UnstableGenetics
{
    /// "组装基因"窗口
    [HarmonyPatch(typeof(Dialog_CreateXenogerm), "DrawSection")]
    public static class Patch_Dialog_UnstableSection
    {
        private static readonly AccessTools.FieldRef<Dialog_CreateXenogerm, Building_GeneAssembler> AssemblerRef =
            AccessTools.FieldRefAccess<Dialog_CreateXenogerm, Building_GeneAssembler>("geneAssembler");

        private static readonly AccessTools.FieldRef<Dialog_CreateXenogerm, List<Genepack>> SelectedRef =
            AccessTools.FieldRefAccess<Dialog_CreateXenogerm, List<Genepack>>("selectedGenepacks");

        private static Texture2D unknownIcon;
        private static bool iconLoaded;

        private static void Postfix(Dialog_CreateXenogerm __instance, Rect rect, bool adding, ref float curY, ref float sectionHeight)
        {
            if (adding)
            {
                return;
            }
            if (!(AssemblerRef(__instance) is Building_UnstableGeneassembler assembler))
            {
                return;
            }

            if (!iconLoaded)
            {
                iconLoaded = true;
                unknownIcon = ContentFinder<Texture2D>.Get("UI/Genes/UnknownUnstableGene", false);
            }

            List<GeneDef> preset = assembler.presetUnstableGenes;
            bool hasPreset = preset != null && preset.Count > 0;

            int unstableCount;
            if (hasPreset)
            {
                unstableCount = preset.Count;
            }
            else
            {
                List<Genepack> selected = SelectedRef(__instance);
                int selectedGeneCount = 0;
                if (selected != null)
                {
                    foreach (Genepack pack in selected)
                    {
                        if (pack?.GeneSet != null)
                        {
                            selectedGeneCount += pack.GeneSet.GenesListForReading.Count;
                        }
                    }
                }
                assembler.RefreshRandomUnstable(selectedGeneCount);
                unstableCount = assembler.randomUnstableGenes == null ? 0 : assembler.randomUnstableGenes.Count;
            }

            float packWidth = 40f + GeneCreationDialogBase.GeneSize.x + 4f * 3f;
            float packHeight = GeneCreationDialogBase.GeneSize.y + 12f;

            Rect labelRect = new Rect(10f, curY, rect.width - 26f, Text.LineHeight);
            Widgets.Label(labelRect, "UG_UnstableGenes".Translate());
            curY += Text.LineHeight + 3f;

            Rect bgRect = new Rect(0f, curY, rect.width, packHeight + 8f);
            Widgets.DrawRectFast(bgRect, Widgets.MenuSectionBGFillColor);
            if (unstableCount == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(bgRect, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            curY += 4f;

            float iconWidth = Text.LineHeightOf(GameFont.Small);
            float x = 4f;
            for (int i = 0; i < unstableCount; i++)
            {
                Rect packRect = new Rect(x, curY, packWidth, packHeight);
                DrawPackBackground(packRect);

                Rect geneRect = new Rect(
                    packRect.x + 38f,
                    packRect.y + 4f,
                    GeneCreationDialogBase.GeneSize.x,
                    GeneCreationDialogBase.GeneSize.y);

                if (hasPreset)
                {
                    GeneDef gene = preset[i];

                    float innerX = packRect.x + 4f;
                    GeneUIUtility.DrawBiostats(gene.biostatCpx, gene.biostatMet, 0, ref innerX, packRect.y, 4f);

                    // 原版绘制：背景
                    GeneUIUtility.DrawGeneDef(gene, geneRect, GeneType.Xenogene, null, doBackground: false, clickable: false);
                }
                else
                {
                    float statY = packRect.y + 6f;
                    DrawUnknownStat(new Rect(packRect.x + 4f, statY, iconWidth, iconWidth), GeneUtility.GCXTex, iconWidth);
                    DrawUnknownStat(new Rect(packRect.x + 4f, statY + GeneCreationDialogBase.GeneSize.y / 3f, iconWidth, iconWidth), GeneUtility.METTex, iconWidth);

                    if (unknownIcon != null)
                    {
                        GUI.DrawTexture(geneRect.ContractedBy(2f), unknownIcon, ScaleMode.ScaleToFit);
                    }

                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(new Rect(geneRect.x, geneRect.yMax - 16f, geneRect.width, 16f), "UG_UnknownUnstableGene".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;

                    TooltipHandler.TipRegion(packRect, "UG_UnknownUnstableGeneDesc".Translate());
                }

                x += packWidth + 14f;
            }

            curY += packHeight + 12f;
            if (Event.current.type == EventType.Layout)
            {
                sectionHeight += packHeight + 12f + Text.LineHeight + 7f;
            }
        }

        private static void DrawPackBackground(Rect packRect)
        {
            //外框
            Widgets.DrawHighlight(packRect);
            GUI.color = new Color(1f, 1f, 1f, 0.1f);
            Widgets.DrawBox(packRect);
            GUI.color = Color.white;
        }

        private static void DrawUnknownStat(Rect iconRect, CachedTexture icon, float iconWidth)
        {
            if (icon != null && icon.Texture != null)
            {
                GUI.DrawTexture(iconRect, icon.Texture);
            }
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.LabelFit(new Rect(iconRect.xMax, iconRect.y, 38f - iconWidth, iconWidth), "?");
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
