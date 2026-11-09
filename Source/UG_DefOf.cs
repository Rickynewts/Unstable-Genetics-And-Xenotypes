using RimWorld;
using Verse;

namespace UnstableGenetics
{
    ///管理Def引用
    [DefOf]
    public static class UG_DefOf
    {
        public static HediffDef UG_EpilepticSeizure;
        public static HediffDef Alzheimers;//阿尔兹海默症
        public static ThoughtDef UG_PostSeizureDistress;//心情减益
        public static GeneDef UG_Forgetful;//健忘基因
        public static GeneDef UG_HypertrophicScar;//增生性疤痕
        public static GeneDef UG_WaterTerror;//恐水基因
        public static ThoughtDef UG_WaterTerrorThought;//恐水心情
        public static HediffDef UG_WaterTerrorFear;//恐水恐慌（水中减速）
        public static ThingDef UnstableGeneassembler;//不稳定基因装配器
        public static ThingDef UnstableXenogerm;//不稳定异种胚芽贴图
        public static JobDef UG_AssembleUnstableXenogerm;//组装不稳定异种胚芽

        static UG_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(UG_DefOf));
        }
    }
}