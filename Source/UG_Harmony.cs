using System.Reflection;
using HarmonyLib;
using Verse;

namespace UnstableGenetics
{
    ///Harmony补丁
    public class UnstableGeneticsMod : Mod
    {
        public UnstableGeneticsMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Yushiren.UnstableGenetics");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
