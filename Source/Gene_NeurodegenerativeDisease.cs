using Verse;

namespace UnstableGenetics
{
    /// 神经退行性疾病：直接复用原版「阿尔兹海默症」（Alzheimers，Hediffs_Local_Chronic）。
    /// 病情会按原版机制每天缓慢加重：大脑效率下降、出现神志混乱、逐渐遗忘记忆。
    public class Gene_NeurodegenerativeDisease : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();

            if (pawn == null || pawn.Dead)
            {
                return;
            }

            if (UG_DefOf.Alzheimers == null || pawn.health.hediffSet.HasHediff(UG_DefOf.Alzheimers))
            {
                return;
            }

            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            pawn.health.AddHediff(UG_DefOf.Alzheimers, brain);
        }
    }
}