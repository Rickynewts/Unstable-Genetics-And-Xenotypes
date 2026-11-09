using UnityEngine;
using Verse;

namespace UnstableGenetics
{
    /// 皱缩皮肤
    /// 沿用原版毛皮渲染节点
    /// 把着色从发色改成皮色——原版毛皮节点固定用 pawn.story.HairColor
    public class PawnRenderNode_WrinkledSkin : PawnRenderNode_Fur
    {
        public PawnRenderNode_WrinkledSkin(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Color ColorFor(Pawn pawn)
        {
            if (pawn.story == null)
            {
                return Color.white;
            }
            return pawn.story.SkinColor;
        }
    }
}