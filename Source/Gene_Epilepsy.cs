using Verse;

namespace UnstableGenetics
{
    /// 基因class
    public class Gene_Epilepsy : Gene
    {
        // 平均发作间隔(天)。2%/天 → MTB = 50 天
        private const float MTBDays = 50f;

        // 一个游戏日的 tick 数
        private const float TicksPerDay = 60000f;

        // 判定间隔(tick)。与 delta 配合
        private const int CheckInterval = 250;

        private ModExtension_Epilepsy cachedExtension;
        private bool extensionLoaded;

        private ModExtension_Epilepsy Extension
        {
            get
            {
                if (!extensionLoaded)
                {
                    cachedExtension = def.GetModExtension<ModExtension_Epilepsy>();
                    extensionLoaded = true;
                }
                return cachedExtension;
            }
        }

        // 只重写 TickInterval，不重写 Tick：
        // Tick() 每 tick 都被调用；TickInterval(delta) 会跟玩家的更新间隔一起批量调用，
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.health?.hediffSet == null)
            {
                return;
            }

            // 原版推荐的批量判定写法：delta = 1 时每 250 tick 触发一次；
            // 更新间隔调大后 delta 变大，仍然保持每 250 tick 一次。
            if (!pawn.IsHashIntervalTick(CheckInterval, delta))
            {
                return;
            }

            // 已经在发作中就不重复触发
            if (pawn.health.hediffSet.HasHediff(UG_DefOf.UG_EpilepticSeizure))
            {
                return;
            }

            // 250 tick 的判定窗口，等价于每天 2%
            if (Rand.MTBEventOccurs(MTBDays, TicksPerDay, CheckInterval))
            {
                TriggerSeizure();
            }
        }

        private void TriggerSeizure()
        {
            Hediff hediff = pawn.health.AddHediff(UG_DefOf.UG_EpilepticSeizure);
            if (hediff == null)
            {
                return;
            }

            // 从 ModExtension 读取持续时间并覆盖 Hediff 里的默认值
            int duration = Extension != null ? Extension.seizureDurationTicks : 0;
            if (duration <= 0)
            {
                return;
            }

            HediffComp_EpilepticSeizure comp = hediff.TryGetComp<HediffComp_EpilepticSeizure>();
            if (comp != null)
            {
                comp.SetDuration(duration);
            }
        }
    }
}