using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace UnstableGenetics
{
    /// <summary>
    /// 不稳定基因装配器的专用 WorkGiver。
    /// </summary>
    public class WorkGiver_UnstableAssembleXenogerm : WorkGiver_Scanner
    {
        /// <summary>
        ///真正的工作点是建筑的交互格，而不是建筑本体。
        ///声明出来可以让 JobGiver_Work 在挑选目标时就用正确的可达性判断
        /// </summary>
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(UG_DefOf.UnstableGeneassembler);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            // 没有 Biotech 时装配器本身就不该存在，直接跳过可以省掉整轮扫描。
            return !ModsConfig.BiotechActive || base.ShouldSkip(pawn, forced);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_UnstableGeneassembler assembler) || !assembler.Spawned)
            {
                return false;
            }

            //断电时既不能组装，也没必要往里面搬材料。
            if (!assembler.Working || !assembler.PowerOn)
            {
                return false;
            }

            //无法到达交互格，返回
            if (!pawn.CanReach(assembler.InteractionCell, PathEndMode.OnCell, Danger.Deadly))
            {
                return false;
            }

            if (!pawn.CanReserve(assembler, 1, -1, null, forced)
                || !pawn.CanReserveSittableOrSpot(assembler.InteractionCell, forced))
            {
                return false;
            }

            if (assembler.MaterialsSatisfied)
            {
                AcceptanceReport report = assembler.CanBeWorkedOnNow;
                if (!report.Accepted)
                {
                    // 例如基因库断电、基因处理器不足、总复杂度超过上限：
                    // 把真实原因带到右键菜单，而不是只显示“无法工作”。
                    if (!report.Reason.NullOrEmpty())
                    {
                        JobFailReason.Is(report.Reason);
                    }
                    return false;
                }
                return true;
            }

            if (FindMaterial(pawn, assembler, forced) != null)
            {
                return true;
            }

            //无材料给出明确原因。
            JobFailReason.Is("MissingMaterials".Translate(MissingMaterialLabel(assembler)));
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_UnstableGeneassembler assembler))
            {
                return null;
            }

            // 与 HasJobOnThing 的前置条件保持一致，避免派发一个必然失败的组装工作。
            if (!assembler.Working || !assembler.PowerOn)
            {
                return null;
            }

            if (!assembler.MaterialsSatisfied)
            {
                Thing material = FindMaterial(pawn, assembler, forced);
                if (material != null)
                {
                    int count = Mathf.Min(assembler.MissingCount(material.def), material.stackCount);
                    if (count > 0)
                    {
                        Job haul = JobMaker.MakeJob(JobDefOf.HaulToContainer, material, t);
                        haul.count = count;
                        return haul;
                    }
                }

                // 材料在两次判断之间被别的搬运工拿走了：
                // 这一轮先不派活，返回 null 让下一轮重新扫描，
                // 而不是派发一个会因为缺材料立刻失败的工作。
                JobFailReason.Is("MissingMaterials".Translate(MissingMaterialLabel(assembler)));
                return null;
            }

            return JobMaker.MakeJob(UG_DefOf.UG_AssembleUnstableXenogerm, t);
        }

        /// <summary>
        /// 按“人肉 → 中性胺”的顺序，返回第一份还缺、并且拿得到的材料。
        /// 已经够数的材料不会触发搜索。
        /// </summary>
        private static Thing FindMaterial(Pawn pawn, Building_UnstableGeneassembler assembler, bool forced)
        {
            ThingDef meat = Building_UnstableGeneassembler.MeatDef;
            if (meat != null && assembler.MissingCount(meat) > 0)
            {
                Thing found = Find(pawn, meat, forced);
                if (found != null)
                {
                    return found;
                }
            }

            ThingDef neutroamine = Building_UnstableGeneassembler.NeutroamineDef;
            if (neutroamine != null && assembler.MissingCount(neutroamine) > 0)
            {
                return Find(pawn, neutroamine, forced);
            }

            return null;
        }

        /// <summary>
        /// 缺少的材料名，仅用于失败提示。
        /// </summary>
        private static string MissingMaterialLabel(Building_UnstableGeneassembler assembler)
        {
            ThingDef meat = Building_UnstableGeneassembler.MeatDef;
            if (meat != null && assembler.MissingCount(meat) > 0)
            {
                return meat.label;
            }

            ThingDef neutroamine = Building_UnstableGeneassembler.NeutroamineDef;
            if (neutroamine != null && assembler.MissingCount(neutroamine) > 0)
            {
                return neutroamine.label;
            }

            return assembler.def.label;
        }

        private static Thing Find(Pawn pawn, ThingDef thingDef, bool forced)
        {
            if (thingDef == null || pawn.Map == null)
            {
                return null;
            }

            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(thingDef),
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn),
                9999f,
                // forced 与 HasJobOnThing 中的预留判断保持一致：
                // 玩家手动“优先工作”时允许忽略其他殖民者的预留。
                x => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, forced));
        }
    }
}
