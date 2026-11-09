using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace UnstableGenetics
{
    /// 组装读条：走到装配器前，按研究速度与装配速度累加进度，
    /// 完成后先扣掉材料，再让建筑生成胚芽。
    public class JobDriver_UnstableAssembleXenogerm : JobDriver
    {
        private const int JobEndInterval = 4000;

        private Building_UnstableGeneassembler Assembler => (Building_UnstableGeneassembler)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Assembler, job, 1, -1, null, errorOnFailed))
            {
                return pawn.ReserveSittableOrSpot(Assembler.InteractionCell, job, errorOnFailed);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !Assembler.CanBeWorkedOnNow.Accepted);
            this.FailOn(() => !Assembler.MaterialsSatisfied);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil toil = ToilMaker.MakeToil("UG_UnstableAssemble");
            toil.tickIntervalAction = (int delta) =>
            {
                float workAmount = pawn.GetStatValue(StatDefOf.ResearchSpeed)
                    * Assembler.GetStatValue(StatDefOf.AssemblySpeedFactor)
                    * (float)delta;
                Assembler.DoWork(workAmount);
                pawn.skills.Learn(SkillDefOf.Intellectual, 0.1f * (float)delta);
                pawn.GainComfortFromCellIfPossible(delta, chairsOnly: true);

                if (Assembler.ProgressPercent >= 1f)
                {
                    Assembler.ConsumeMaterials();
                    Assembler.CompleteAssembly();
                    pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            toil.WithEffect(EffecterDefOf.GeneAssembler_Working, TargetIndex.A);
            toil.WithProgressBar(TargetIndex.A, () => Assembler.ProgressPercent, interpolateBetweenActorAndTarget: false, 0f);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.defaultDuration = JobEndInterval;
            toil.activeSkill = () => SkillDefOf.Intellectual;
            yield return toil;
        }
    }
}