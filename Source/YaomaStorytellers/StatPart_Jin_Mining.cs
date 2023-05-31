using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
	public class StatPart_Jin_Mining : StatPart
    {
		public override void TransformValue(StatRequest req, ref float val)
		{
			bool hasThing = req.HasThing;
			if (hasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				bool flag = pawn != null && Find.Storyteller.def == StorytellerDefOf.JianghuJin_Yaoma;
				if (flag)
				{
					val *= YaomaStorytellerUtility.settings.JianghuJinMiningBoost;
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			bool hasThing = req.HasThing;
			if (hasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				bool flag = pawn != null && Find.Storyteller.def == StorytellerDefOf.JianghuJin_Yaoma;
				if (flag)
				{
					return "YS_StatPart_JinMining".Translate() + " x" + YaomaStorytellerUtility.settings.JianghuJinMiningBoost.ToStringPercent();
				}
			}
			return null;
		}
	}
	
}
