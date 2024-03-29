﻿using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class StatPart_Jin_Construct : StatPart
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
					val *= YaomaStorytellerUtility.settings.JianghuJinConstructBoost;
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
					return "YS_StatPart_JinConstruct".Translate() + " x" + YaomaStorytellerUtility.settings.JianghuJinConstructBoost.ToStringPercent();
				}
			}
			return null;
		}
	}
	
}
