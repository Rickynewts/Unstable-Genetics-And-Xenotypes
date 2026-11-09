using RimWorld;
using Verse;

namespace UnstableGenetics
{
    ///发作倒计时
    public class HediffComp_EpilepticSeizure : HediffComp
    {
        private int ticksLeft;

        public HediffCompProperties_EpilepticSeizure Props =>
            (HediffCompProperties_EpilepticSeizure)props;

        // 用原版的 ShouldRemove 机制结束发作，不在 comp tick 里直接删 Hediff
        public override bool CompShouldRemove => base.CompShouldRemove || ticksLeft <= 0;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ticksLeft = Props.defaultDurationTicks;
        }

        /// 由基因侧覆盖持续时间
        public void SetDuration(int ticks)
        {
            if (ticks > 0)
            {
                ticksLeft = ticks;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (ticksLeft > 0)
            {
                ticksLeft -= delta;
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            // 只有自然到点才给心情；被其他原因移除（死亡、调试等）不给
            if (ticksLeft <= 0)
            {
                ApplyPostSeizureThought();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            // 必须存档：否则读档后倒计时归零。
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", Props.defaultDurationTicks);
        }

        private void ApplyPostSeizureThought()
        {
            if (Pawn == null || Pawn.Dead)
            {
                return;
            }
            MemoryThoughtHandler memories = Pawn.needs?.mood?.thoughts?.memories;
            if (memories == null || UG_DefOf.UG_PostSeizureDistress == null)
            {
                return;
            }
            memories.TryGainMemory(UG_DefOf.UG_PostSeizureDistress);
        }

        public override string CompDebugString()
        {
            return "ticksLeft: " + ticksLeft;
        }
    }
}