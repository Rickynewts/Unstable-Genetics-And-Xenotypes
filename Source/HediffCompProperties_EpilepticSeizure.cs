using Verse;

namespace UnstableGenetics
{
    ///Hediff的comp属性
    public class HediffCompProperties_EpilepticSeizure : HediffCompProperties
    {   //持续时间
        public int defaultDurationTicks = 7200;
        public HediffCompProperties_EpilepticSeizure()
        {   //comp
            compClass = typeof(HediffComp_EpilepticSeizure);
        }
    }
}