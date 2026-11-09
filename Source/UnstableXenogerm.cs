using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace UnstableGenetics
{
    /// 不稳定异种胚芽：沿用原版 Xenogerm 的全部行为，
    public class UnstableXenogerm : Xenogerm
    {
        private static Texture2D implantIcon;
        private static bool implantIconLoaded;

        private static Texture2D ImplantIcon
        {
            get
            {
                if (!implantIconLoaded)
                {
                    implantIconLoaded = true;
                    implantIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/ImplantUnstable Genes", false);
                }
                return implantIcon;
            }
        }

        private static readonly string ImplantLabel = "ImplantXenogerm".Translate() + "...";

        private static Texture2D inspectIcon;
        private static bool inspectIconLoaded;

        private static Texture2D InspectIcon
        {
            get
            {
                if (!inspectIconLoaded)
                {
                    inspectIconLoaded = true;
                    inspectIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/ViewUnstableGenes", false);
                }
                return inspectIcon;
            }
        }

        private static readonly string InspectLabel = "InspectGenes".Translate() + "...";

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action command)
                {
                    if (command.defaultLabel == ImplantLabel && ImplantIcon != null)
                    {
                        command.icon = ImplantIcon;
                    }
                    else if (command.defaultLabel == InspectLabel && InspectIcon != null)
                    {
                        command.icon = InspectIcon;
                    }
                }
                yield return gizmo;
            }
        }
    }
}