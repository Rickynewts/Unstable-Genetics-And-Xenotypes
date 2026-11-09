using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UnstableGenetics
{
    /// 继承原版 Building_GeneAssembler
    public class Building_UnstableGeneassembler : Building_GeneAssembler
    {
        public const int MeatRequired = 20;
        public const int NeutroamineRequired = 50;

        /// 固定上限。
        public const int ComplexityLimit = 30;

        public static readonly string[] UnstableGeneDefNames =
        {
            "UG_Epilepsy", "UG_Forgetful", "UG_FragileSpine", "UG_KidneyFailure",
            "UG_Neurasthenia", "UG_NeurodegenerativeDisease", "UG_Photophobia", "UG_SensitiveStomach",
            "UG_WaterTerror", "UG_Cataract", "UG_HypertrophicScar", "UG_ArteryBlockage", "UG_HearingLoss"
        };

        /// <summary>
        /// 名字集合，避免在遍历全部 GeneDef 时对每个 def 做线性字符串比较。
        /// </summary>
        internal static readonly HashSet<string> UnstableGeneDefNameSet =
            new HashSet<string>(UnstableGeneDefNames, System.StringComparer.Ordinal);

        /// <summary>
        /// 不稳定基因池只和 Def 有关，缓存一次即可。
        /// 对话框里每改一次选择都会调用 RefreshRandomUnstable，
        /// 之前每次都重新做一遍 LINQ + DefDatabase 查询。
        /// </summary>
        private static List<GeneDef> cachedUnstableGenePool;

        private static List<GeneDef> UnstableGeneDefPool
        {
            get
            {
                if (cachedUnstableGenePool != null)
                {
                    return cachedUnstableGenePool;
                }

                List<GeneDef> pool = new List<GeneDef>(UnstableGeneDefNames.Length);
                for (int i = 0; i < UnstableGeneDefNames.Length; i++)
                {
                    GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(UnstableGeneDefNames[i]);
                    if (gene != null)
                    {
                        pool.Add(gene);
                    }
                }

                // Def 尚未加载完时不要把空池缓存下来。
                if (pool.Count > 0)
                {
                    cachedUnstableGenePool = pool;
                }
                return pool;
            }
        }

        ///从预设异种载入
        public List<GeneDef> presetUnstableGenes;

        ///依然计算代谢率
        public List<GeneDef> randomUnstableGenes;

        ///按已选基因数刷新随机缺陷基因
        public void RefreshRandomUnstable(int selectedGeneCount)
        {
            int count = UnstableCountFor(selectedGeneCount);
            if (count <= 0)
            {
                randomUnstableGenes = null;
                return;
            }
            if (randomUnstableGenes != null && randomUnstableGenes.Count == count)
            {
                return;
            }

            List<GeneDef> pool = new List<GeneDef>(UnstableGeneDefPool);
            randomUnstableGenes = new List<GeneDef>(Mathf.Min(count, pool.Count));
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                GeneDef gene = pool.RandomElement();
                pool.Remove(gene);
                randomUnstableGenes.Add(gene);
            }
        }

        private static ThingDef cachedNeutroamine;
        private List<Genepack> cachedVirtualGenepacks;

        public static ThingDef NeutroamineDef =>
            cachedNeutroamine ?? (cachedNeutroamine = DefDatabase<ThingDef>.GetNamedSilentFail("Neutroamine"));

        public static ThingDef MeatDef => ThingDefOf.Meat_Human;

        private static readonly MethodInfo ResetMethod =
            typeof(Building_GeneAssembler).GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly System.Action<Building_GeneAssembler> ResetAction =
            ResetMethod == null
                ? null
                : (System.Action<Building_GeneAssembler>)System.Delegate.CreateDelegate(typeof(System.Action<Building_GeneAssembler>), ResetMethod);

        /// 本次组装要写入的基因，用 Def 列表存档，读档后仍能恢复。
        public List<GeneDef> pendingGenes;

        private static readonly AccessTools.FieldRef<Building_GeneAssembler, List<Genepack>> GenepacksRef =
            AccessTools.FieldRefAccess<Building_GeneAssembler, List<Genepack>>("genepacksToRecombine");

        private static readonly AccessTools.FieldRef<Building_GeneAssembler, int?> ComplexityRef =
            AccessTools.FieldRefAccess<Building_GeneAssembler, int?>("cachedComplexity");

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pendingGenes, "UG_pendingGenes", LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Working)
            {
                if (!pendingGenes.NullOrEmpty())
                {
                    GenepacksRef(this) = GetPacksForGenes(pendingGenes);
                    ComplexityRef(this) = null;
                }
                else
                {
                    ResetSelf();
                }
            }
        }

        public void ResetSelf()
        {
            ResetAction?.Invoke(this);
        }

        ///不稳定异种胚芽
        public void CompleteAssembly()
        {
            if (pendingGenes.NullOrEmpty())
            {
                Messages.Message("UG_AssemblyDataLost".Translate(), this, MessageTypeDefOf.NegativeEvent, false);
                ResetSelf();
                return;
            }

            List<Genepack> packs = new List<Genepack>();
            foreach (GeneDef gene in pendingGenes)
            {
                Genepack pack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                pack.Initialize(new List<GeneDef> { gene });
                packs.Add(pack);
            }

            string name = xenotypeName;
            XenotypeIconDef icon = iconDef ?? XenotypeIconDefOf.Basic;
            pendingGenes = null;

            SoundDefOf.GeneAssembler_Complete.PlayOneShot(SoundInfo.InMap(this));

            Xenogerm xenogerm = (Xenogerm)ThingMaker.MakeThing(UG_DefOf.UnstableXenogerm);
            xenogerm.Initialize(packs, name, icon);
            if (GenPlace.TryPlaceThing(xenogerm, InteractionCell, Map, ThingPlaceMode.Near))
            {
                Messages.Message("UG_XenogermAssemblyComplete".Translate(), xenogerm, MessageTypeDefOf.PositiveEvent);
            }
            ResetSelf();
        }


        /// 全部原版基因
        public List<Genepack> GetVirtualGenepacks()
        {
            if (cachedVirtualGenepacks != null)
            {
                return cachedVirtualGenepacks;
            }

            cachedVirtualGenepacks = new List<Genepack>();
            foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                // 排除超凡胶囊需求
                if (gene.biostatArc > 0 || !gene.canGenerateInGeneSet)
                {
                    continue;
                }
                if (gene.modContentPack == null)
                {
                    continue;
                }

                bool isOfficial = gene.modContentPack.IsOfficialMod;
                bool isOurCosmetic = string.Equals(gene.modContentPack.PackageId, "yushiren.unstablegenetics", System.StringComparison.OrdinalIgnoreCase)
                    && !UnstableGeneDefNameSet.Contains(gene.defName);
                if (!isOfficial && !isOurCosmetic)
                {
                    continue;
                }

                Genepack pack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                pack.Initialize(new List<GeneDef> { gene });
                cachedVirtualGenepacks.Add(pack);
            }
            return cachedVirtualGenepacks;
        }

        ///每4个已选基因追加 1 个不稳定基因
        public List<Genepack> GetPacksForGenes(List<GeneDef> genes)

        {

            List<Genepack> packs = new List<Genepack>();

            if (genes == null)

            {

                return packs;

            }

            foreach (GeneDef gene in genes)

            {

                Genepack pack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);

                pack.Initialize(new List<GeneDef> { gene });

                packs.Add(pack);

            }

            return packs;

        }


        public static int UnstableCountFor(int selectedGeneCount)
        {
            if (selectedGeneCount <= 0)
            {
                return 0;
            }
            return Mathf.Max(1, Mathf.CeilToInt(selectedGeneCount / 4f));
        }

        ///随机不稳定基因
        public List<Genepack> GetRandomUnstablePacks(int count)
        {
            // 池是缓存好的，这里复制一份再随机抽取，避免改动缓存本身。
            List<GeneDef> pool = new List<GeneDef>(UnstableGeneDefPool);

            List<Genepack> result = new List<Genepack>(Mathf.Min(count, pool.Count));
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                GeneDef gene = pool.RandomElement();
                pool.Remove(gene);

                Genepack pack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                pack.Initialize(new List<GeneDef> { gene });
                result.Add(pack);
            }
            return result;
        }

        public int CountMaterial(ThingDef thingDef)
        {
            if (innerContainer == null || thingDef == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (innerContainer[i].def == thingDef)
                {
                    num += innerContainer[i].stackCount;
                }
            }
            return num;
        }

        public int MissingCount(ThingDef thingDef)
        {
            int required = thingDef == MeatDef ? MeatRequired : NeutroamineRequired;
            return Mathf.Max(0, required - CountMaterial(thingDef));
        }

        public bool MaterialsSatisfied =>
            CountMaterial(MeatDef) >= MeatRequired
            && NeutroamineDef != null
            && CountMaterial(NeutroamineDef) >= NeutroamineRequired;

        ///扣材料
        public void ConsumeMaterials()
        {
            Consume(MeatDef, MeatRequired);
            if (NeutroamineDef != null)
            {
                Consume(NeutroamineDef, NeutroamineRequired);
            }
        }

        private void Consume(ThingDef thingDef, int count)
        {
            int remaining = count;
            for (int i = innerContainer.Count - 1; i >= 0 && remaining > 0; i--)
            {
                Thing thing = innerContainer[i];
                if (thing.def != thingDef)
                {
                    continue;
                }
                int take = Mathf.Min(remaining, thing.stackCount);
                Thing part = thing.SplitOff(take);
                remaining -= take;
                part.Destroy(DestroyMode.Vanish);
            }
        }

        private static Texture2D assembleIcon;
        private static bool assembleIconLoaded;

        private static Texture2D AssembleIcon
        {
            get
            {
                if (!assembleIconLoaded)
                {
                    assembleIconLoaded = true;
                    assembleIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/RecombineUnstableGenes", false);
                }
                return assembleIcon;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                // 原版按钮的 label 是 "Recombine".Translate() + "..."（即"重组..."）
                if (gizmo is Command_Action cmd && cmd.defaultLabel == "Recombine".Translate() + "...")
                {
                    if (AssembleIcon != null)
                    {
                        cmd.icon = AssembleIcon;
                    }

                    cmd.defaultLabel = "UG_AssembleXenogermGizmo".Translate();
                    cmd.defaultDesc = "UG_AssembleXenogermGizmoDesc".Translate(
                        MeatRequired,
                        NeutroamineRequired,
                        CountMaterial(MeatDef),
                        NeutroamineDef == null ? 0 : CountMaterial(NeutroamineDef));

                   // 材料不足不禁用按钮
                }
                yield return gizmo;
            }
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (!Working)
            {
                int meat = CountMaterial(MeatDef);
                int neut = NeutroamineDef == null ? 0 : CountMaterial(NeutroamineDef);
                text += (text.NullOrEmpty() ? "" : "\n")
                    + "UG_MaterialsInspect".Translate(meat, MeatRequired, neut, NeutroamineRequired);
            }
            return text;
        }
    }
}