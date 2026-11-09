using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace UnstableGenetics
{
    ///删除右键菜单
    [HarmonyPatch(typeof(Dialog_CreateXenogerm), "DrawGenepack")]
    public static class Patch_Dialog_NoEjectMenu
    {
        private static readonly AccessTools.FieldRef<Dialog_CreateXenogerm, Building_GeneAssembler> AssemblerRef =
            AccessTools.FieldRefAccess<Dialog_CreateXenogerm, Building_GeneAssembler>("geneAssembler");

        private static void Prefix(Dialog_CreateXenogerm __instance, ref float curX, float curY, float packWidth)
        {
            if (!(AssemblerRef(__instance) is Building_UnstableGeneassembler))
            {
                return;
            }

            Event current = Event.current;
            if (current == null || current.button != 1)
            {
                return;
            }
            if (current.type != EventType.MouseDown && current.type != EventType.MouseUp)
            {
                return;
            }

            Rect packRect = new Rect(curX, curY, packWidth, GeneCreationDialogBase.GeneSize.y + 8f);
            if (Mouse.IsOver(packRect))
            {
                current.type = EventType.Used;
            }
        }
    }
}