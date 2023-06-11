using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace YaomaStorytellers
{
	public class IncidentWorker_Terraform_Jin : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (Find.Storyteller.def != StorytellerDefOf.JianghuJin_Yaoma) return false;
			return true; // adjust so that it only fires when Jin is the storyteller
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = Find.AnyPlayerHomeMap;
			if (map is null) return false;

			foreach (var step in MapGeneratorDefOf_Yaoma.YS_JianghuJin_RefreshTerrain.genSteps.OrderBy(x => x.order))
			{
				DeepProfiler.Start(step.genStep.def.defName);
				step.genStep.Generate(map, default(GenStepParams));
				DeepProfiler.End();
			}

			map.FinalizeInit();

			base.SendStandardLetter("YS_LetterLabelJianghuJin".Translate(), "YS_LetterJianghuJin".Translate(),
				LetterDefOf.NeutralEvent, parms, null, Array.Empty<NamedArgument>());

			return true;

		}
	}
}
